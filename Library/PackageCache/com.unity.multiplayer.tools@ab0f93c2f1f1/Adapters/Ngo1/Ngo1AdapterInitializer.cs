using System.Threading;
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
#if UNITY_NETCODE_GAMEOBJECTS_2_1_0_ABOVE
        static bool s_Initialized;
#endif
        static Ngo1Adapter s_Instance;
        static CancellationTokenSource s_InitializeAdapterCts;

#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void ResetStaticsOnLoad()
        {
            s_InitializeAdapterCts?.Cancel();
            s_InitializeAdapterCts = null;
            if (s_Instance != null)
            {
                NetworkAdapters.RemoveAdapter(s_Instance);
                s_Instance.Deinitialize();
                s_Instance = null;
            }
#if UNITY_NETCODE_GAMEOBJECTS_2_1_0_ABOVE
            if (s_Initialized)
            {
                NetworkManager.OnInstantiated -= OnInstantiated;
                NetworkManager.OnDestroying -= OnDestroying;
                s_Initialized = false;
            }
#endif
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        internal static void InitializeAdapter()
        {
#if UNITY_NETCODE_GAMEOBJECTS_2_1_0_ABOVE
            // Register NetworkManagers instantiated from now on, including ones recreated on scene changes.
            // OnInstantiated is raised in NetworkManager.Awake.
            if (!s_Initialized)
            {
                NetworkManager.OnInstantiated += OnInstantiated;
                NetworkManager.OnDestroying += OnDestroying;
                s_Initialized = true;
            }
#endif
            // Also a fallback for > 2.1.0 when a NetworkManager's one-shot OnInstantiated already fired before the subscription above ran
            s_InitializeAdapterCts?.Cancel();
            s_InitializeAdapterCts = new CancellationTokenSource();
            InitializeAdapterAsync(s_InitializeAdapterCts.Token).Forget();
        }

        static void RegisterNetworkManager(NetworkManager networkManager)
        {
            if (s_Instance == null)
            {
                s_Instance = new Ngo1Adapter(networkManager);
                NetworkAdapters.AddAdapter(s_Instance);
            }
            else
            {
                s_Instance.ReplaceNetworkManager(networkManager);
            }
        }

#if UNITY_NETCODE_GAMEOBJECTS_2_1_0_ABOVE
        static void OnInstantiated(NetworkManager networkManager)
        {
            // The event owns registration from here on; stop the startup poll so it can't fire later.
            s_InitializeAdapterCts?.Cancel();
            RegisterNetworkManager(networkManager);
        }

        static void OnDestroying(NetworkManager _)
        {
            if (s_Instance != null)
            {
                NetworkAdapters.RemoveAdapter(s_Instance);
                s_Instance.Deinitialize();
                s_Instance = null;
            }
        }
#endif

        static async Task InitializeAdapterAsync(CancellationToken ct)
        {
            var networkManager = await GetNetworkManagerAsync(ct);
            if (ct.IsCancellationRequested || networkManager == null)
            {
                return;
            }
            RegisterNetworkManager(networkManager);
        }

        static async Task<NetworkManager> GetNetworkManagerAsync(CancellationToken ct)
        {
            while (NetworkManager.Singleton == null || NetworkManager.Singleton.NetworkTickSystem == null)
            {
                if (ct.IsCancellationRequested)
                {
                    return null;
                }
                await Task.Yield();
            }
            return NetworkManager.Singleton;
        }
    }
}
