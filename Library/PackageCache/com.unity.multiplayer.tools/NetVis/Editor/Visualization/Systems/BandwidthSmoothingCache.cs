using System;
using System.Collections.Generic;
using Unity.Multiplayer.Tools.Adapters;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.NetVis.Configuration;

namespace Unity.Multiplayer.Tools.NetVis.Editor.Visualization
{
    /// <remarks>
    /// The math in this class is equivalent to <see cref="ContinuousExponentialMovingAverage"/>,
    /// but the updates are batched to share the decay constant, previous update, and intermediary computations between
    /// multiple objects rather than duplicating these for each object.
    /// </remarks>>
    class BandwidthSmoothingCache
    {
        double m_DecayConstant = 1f;
        BandwidthTypes m_BandwidthType = BandwidthTypes.All;
        NetworkDirection m_Direction = NetworkDirection.None;

        double m_LastUpdateTime = double.MinValue;
        readonly Dictionary<ObjectId, float> m_SmoothedBandwidth = new();

        public bool NeedsResetToImmediateValue { get; private set; }

        public float MaxBandwidth { get; private set; } = 0f;

        public BandwidthSmoothingCache(BandwidthSettings settings)
        {
            OnConfigurationChanged(settings);
        }

        public void OnConfigurationChanged(BandwidthSettings settings)
        {
            if (m_BandwidthType != settings.BandwidthType || m_Direction != settings.NetworkDirection)
            {
                m_BandwidthType = settings.BandwidthType;
                m_Direction = settings.NetworkDirection;

                NeedsResetToImmediateValue = true;
            }

            m_DecayConstant =
                ContinuousExponentialMovingAverage.GetDecayConstantForHalfLife(settings.SmoothingHalfLife);
        }

        public float this[ObjectId objectId] => m_SmoothedBandwidth.TryGetValue(objectId, out var value) ? value : 0f;

        public void Update(IGetObjectIds getObjectIds, IGetBandwidth getBandwidth, double time)
        {
            if (getBandwidth.IsCacheEmpty)
            {
                // Skip first frame where the cache is empty, but still get time to compute delta time
                m_LastUpdateTime = time;
                return;
            }
            
            var deltaTime = time - m_LastUpdateTime;
            m_LastUpdateTime = time;

            var oldValueWeight = (float)Math.Exp(-deltaTime * m_DecayConstant);
            if (NeedsResetToImmediateValue || m_DecayConstant <= 0)
            {
                oldValueWeight = 0;
                NeedsResetToImmediateValue = false;
            }

            var newValueWeight = 1 - oldValueWeight;

            var deltaTimeInverse = deltaTime == 0 ? 1 : (float)(1 / deltaTime);

            var objectCount = 0;
            MaxBandwidth = 0f;
            foreach (var objectId in getObjectIds.ObjectIds)
            {
                ++objectCount;

                var previousRate = this[objectId];
                var newValue = getBandwidth.GetBandwidthBytes(objectId, m_BandwidthType, m_Direction);
                var newRate = newValue * deltaTimeInverse;

                var newBandwidth = previousRate * oldValueWeight + newRate * newValueWeight;
                MaxBandwidth = MathF.Max(MaxBandwidth, newBandwidth);
                m_SmoothedBandwidth[objectId] = newBandwidth;
            }

            var keyCount = m_SmoothedBandwidth.Count;
            var unusedEntriesExceed20Percent = keyCount * 5 > objectCount * 6; // (keyCount/objectCount) > (6/5)
            if (unusedEntriesExceed20Percent)
            {
                // We need to clear unused entries for objects that have been de-spawned (like projectiles)
                // to avoid unbounded growth. Doing so, however, requires creating a collection of unused keys,
                // which may require allocation.
                // It's possible to avoid doing this work each frame by only clearing unused entries
                // when a certain threshold of unused entries has been reached, such as 20%.
                ClearUnusedBandwidthEntries(getObjectIds);
            }
        }

        HashSet<ObjectId> m_ActiveObjectIds;
        List<ObjectId> m_UnusedObjectIds;
        void ClearUnusedBandwidthEntries(IGetObjectIds getObjectIds)
        {
            m_ActiveObjectIds ??= new();
            m_UnusedObjectIds ??= new();
            foreach (var objectId in getObjectIds.ObjectIds)
            {
                m_ActiveObjectIds.Add(objectId);
            }

            foreach (var key in m_SmoothedBandwidth.Keys)
            {
                if (!m_ActiveObjectIds.Contains(key))
                {
                    m_UnusedObjectIds.Add(key);
                }
            }

            foreach (var unusedObjectId in m_UnusedObjectIds)
            {
                m_SmoothedBandwidth.Remove(unusedObjectId);
            }

            m_ActiveObjectIds.Clear();
            m_UnusedObjectIds.Clear();
        }
    }
}
