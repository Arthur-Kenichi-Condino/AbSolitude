using System.Collections.Generic;
using Unity.Multiplayer.Tools.Adapters.Utp2;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Scripting;

[assembly: AlwaysLinkAssembly]
namespace Unity.Multiplayer.Tools.Adapters.Ngo1WithUtp2
{
    static class Ngo1WithUtp2AdapterInitializer
    {
        static bool s_Initialized;
        // EntityId support requires both NGO 2.8.0+ and Unity 6000.2+.
        // Unity 6000.2+ is required because the EntityId feature is only available in that version or newer.
#if UNITY_NETCODE_GAMEOBJECTS_2_8_ABOVE && UNITY_6000_2_OR_NEWER
        internal static readonly IDictionary<EntityId, Utp2Adapter> s_Adapters = new Dictionary<EntityId, Utp2Adapter>();
#else
        // Use the InstanceId key'd table
        internal static readonly IDictionary<int, Utp2Adapter> s_Adapters = new Dictionary<int, Utp2Adapter>();
#endif

        [RuntimeInitializeOnLoadMethod]
        internal static void InitializeAdapter()
        {
            if (!s_Initialized)
            {
                s_Initialized = true;

#if UNITY_NETCODE_GAMEOBJECTS_2_8_ABOVE && UNITY_6000_2_OR_NEWER
                // Use the EntityId updated actions
                UnityTransport.OnDriverInitialized += AddAdapter;
                UnityTransport.OnDisposingDriver += RemoveAdapter;
#else
                // Use the legacy InstanceId actions
                UnityTransport.TransportInitialized += AddAdapter;
                UnityTransport.TransportDisposed += RemoveAdapter;
#endif
            }
        }

#if UNITY_NETCODE_GAMEOBJECTS_2_8_ABOVE && UNITY_6000_2_OR_NEWER
        static void AddAdapter(EntityId entityId, NetworkDriver networkDriver)
        {
            if (s_Adapters.ContainsKey(entityId))
            {
                return;
            }

            var adapter = new Utp2Adapter(networkDriver);
            s_Adapters[entityId] = adapter;
            NetworkAdapters.AddAdapter(adapter);
        }
#else
        static void AddAdapter(int instanceId, NetworkDriver networkDriver)
        {
            if (s_Adapters.ContainsKey(instanceId))
            {
                return;
            }

            var adapter = new Utp2Adapter(networkDriver);
            s_Adapters[instanceId] = adapter;
            NetworkAdapters.AddAdapter(adapter);
        }
#endif


#if UNITY_NETCODE_GAMEOBJECTS_2_8_ABOVE && UNITY_6000_2_OR_NEWER
        static void RemoveAdapter(EntityId entityId)
        {
            if (s_Adapters.TryGetValue(entityId, out var adapter))
            {
                NetworkAdapters.RemoveAdapter(adapter);
                s_Adapters.Remove(entityId);
            }
        }
#else
        static void RemoveAdapter(int instanceId)
        {
            if (s_Adapters.TryGetValue(instanceId, out var adapter))
            {
                NetworkAdapters.RemoveAdapter(adapter);
                s_Adapters.Remove(instanceId);
            }
        }
#endif
    }
}
