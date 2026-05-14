using System;
using Unity.Multiplayer.Tools.Adapters;
using Unity.Multiplayer.Tools.NetVis.Configuration;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetVis.Editor.Visualization
{
    class ObjectColoring
    {
        static readonly Color k_NoColor = Color.clear;

        NetVisDataStore NetVisDataStore { get; }

        NetVisConfiguration m_Configuration;
        BandwidthSettings BandwidthSettings => m_Configuration.Settings.Bandwidth;
        OwnershipSettings OwnershipSettings => m_Configuration.Settings.Ownership;

        float m_BandwidthMin = 0;
        float m_BandwidthMax = 1;
        float m_BandwidthDelta = 1;

        public ObjectColoring(NetVisDataStore netVisDataStore)
        {
            NetVisDataStore = netVisDataStore;
        }

        /// <remarks>
        /// Update the internal state to prepare for a new frame
        /// </remarks>
        public void UpdateForNewFrame(NetVisConfiguration configuration)
        {
            m_Configuration = configuration;
            switch (m_Configuration.Metric)
            {
                case NetVisMetric.Bandwidth:
                    UpdateBandwidthParametersForNewFrame();
                    break;
                case NetVisMetric.Ownership:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool GetColor(ObjectId objectId, out Color color)
        {
            return m_Configuration.Metric switch
            {
                NetVisMetric.Bandwidth => GetColorForBandwidth(objectId, out color),
                NetVisMetric.Ownership => GetColorForOwnership(objectId, out color),
                NetVisMetric.None or _ => throw new ArgumentOutOfRangeException()
            };
        }

        void UpdateBandwidthParametersForNewFrame()
        {
            m_BandwidthMin = BandwidthSettings.BandwidthAutoscaling
                ? NetVisDataStore.GetMinBandwidth()
                : BandwidthSettings.BandwidthMin;

            m_BandwidthMax = BandwidthSettings.BandwidthAutoscaling
                ? NetVisDataStore.GetMaxBandwidth()
                : BandwidthSettings.BandwidthMaxSafe;

            m_BandwidthDelta = m_BandwidthMax - m_BandwidthMin;
        }

        bool GetColorForBandwidth(ObjectId objectId, out Color color)
        {
            var bandwidth = NetVisDataStore.GetBandwidth(objectId);
            var bandwidthNormalized = (bandwidth - m_BandwidthMin) / m_BandwidthDelta;
            if (bandwidthNormalized < 0)
            {
                color = k_NoColor;
                return false;
            }
            color = BandwidthSettings.MeshShadingFill.Evaluate(bandwidthNormalized);
            return true;
        }

        bool GetColorForOwnership(ObjectId objectId, out Color color)
        {
            var owner = NetVisDataStore.GetOwner(objectId);
            color = owner == 0 ? OwnershipSettings.ServerHostColor : OwnershipSettings.GetClientColor(owner);

            return true;
        }
    }
}
