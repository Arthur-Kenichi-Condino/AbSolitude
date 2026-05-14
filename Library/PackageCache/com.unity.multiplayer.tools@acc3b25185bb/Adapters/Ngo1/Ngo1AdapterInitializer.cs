using System;
using System.Threading.Tasks;
using Unity.Multiplayer.Tools.Common;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Scripting;

[assembly: AlwaysLinkAssembly]
namespace Unity.Multiplayer.Tools.Adapters.Ngo1
{
    static class Ngo1AdapterInitializer
    {
        static bool s_Initialized;

        [RuntimeInitializeOnLoadMethod]
        internal static void InitializeAdapter()
        {
            if (!s_Initialized)
            {
                s_Initialized = true;
                InitializeAdapterAsync().Forget();
            }
        }

        static async Task InitializeAdapterAsync()
        {
            var networkManager = await GetNetworkManagerAsync();
            var ngo1Adapter = new Ngo1Adapter(networkManager);
            NetworkAdapters.AddAdapter(ngo1Adapter);

#if UNITY_NETCODE_GAMEOBJECTS_2_1_0_ABOVE
            // We need the OnInstantiated callback because the NetworkManager could get destroyed and recreated when we change scenes
            // OnInstantiated is called in Awake, and the GetNetworkManagerAsync only returns at least after OnEnable
            // therefore the initialization is not called twice
            NetworkManager.OnInstantiated += async _ =>
            {
                // We need to wait for the NetworkTickSystem to be ready as well
                var newNetworkManager = await GetNetworkManagerAsync();
                ngo1Adapter.ReplaceNetworkManager(newNetworkManager);
            };
            NetworkManager.OnDestroying += _ =>
            {
                ngo1Adapter.Deinitialize();
                // Assure we reset the initialized flag so the next time
                // we want to initialize it will properly initialize the adapter.
                s_Initialized = false;
            };
#endif
        }

        static async Task<NetworkManager> GetNetworkManagerAsync()
        {
            while (NetworkManager.Singleton == null || NetworkManager.Singleton.NetworkTickSystem == null)
            {
                await Task.Yield();
            }

            return NetworkManager.Singleton;
        }
    }
}
