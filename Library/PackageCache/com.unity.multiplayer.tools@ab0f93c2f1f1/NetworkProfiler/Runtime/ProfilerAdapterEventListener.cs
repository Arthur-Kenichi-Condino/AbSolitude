using System.Diagnostics;
using Unity.Multiplayer.Tools.Adapters;
using Unity.Multiplayer.Tools.NetStats;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Scripting;

[assembly: AlwaysLinkAssembly]
namespace Unity.Multiplayer.Tools.NetworkProfiler.Runtime
{
    static class ProfilerAdapterEventListener
    {
        static bool s_Initialized;
        static UnsubscribeFromAllAdapters s_UnsubscribeFromAllAdapters;

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void ResetStaticsOnLoad()
        {            
            s_Initialized = false;
            s_UnsubscribeFromAllAdapters = null;
        }
#endif
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void SubscribeToAdapterAndMetricEvents()
        {
            if (!s_Initialized)
            {
                s_Initialized = true;
                s_UnsubscribeFromAllAdapters = NetworkAdapters.SubscribeToAll(
                    OnAdapterAdded,
                    OnAdapterRemoved);
                Application.quitting += OnApplicationExit;
            }
        }

        static void OnApplicationExit()
        {
            s_UnsubscribeFromAllAdapters?.Invoke();
            Application.quitting -= OnApplicationExit;
        }

        static void OnAdapterAdded(INetworkAdapter adapter)
        {
            var metricEventComponent = adapter.GetComponent<IMetricCollectionEvent>();
            if (metricEventComponent != null)
            {
                metricEventComponent.MetricCollectionEvent += OnMetricsReceived;
            }
        }

        static void OnAdapterRemoved(INetworkAdapter adapter)
        {
            var metricEventComponent = adapter.GetComponent<IMetricCollectionEvent>();
            if (metricEventComponent != null)
            {
                metricEventComponent.MetricCollectionEvent -= OnMetricsReceived;
            }
        }

        static void OnMetricsReceived(MetricCollection metricCollection)
        {
            PopulateProfilerIfEnabled(metricCollection);
        }

        static readonly NetStatSerializer s_NetStatSerializer = new();

        [Conditional("ENABLE_PROFILER")]
        static void PopulateProfilerIfEnabled(MetricCollection collection)
        {
            ProfilerCounters.Instance.UpdateFromMetrics(collection);

            using var result = s_NetStatSerializer.Serialize(collection);
            Profiler.EmitFrameMetaData(
                FrameInfo.NetworkProfilerGuid,
                FrameInfo.NetworkProfilerDataTag,
                result);
        }
    }
}
