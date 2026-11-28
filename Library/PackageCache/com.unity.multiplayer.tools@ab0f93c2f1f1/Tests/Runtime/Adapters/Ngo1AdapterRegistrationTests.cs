using System.Collections;
using System.Linq;
#if UNITY_EDITOR
using System.Reflection;
#endif
using NUnit.Framework;
using Unity.Multiplayer.Tools.Adapters.Ngo1;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.Multiplayer.Tools.Adapters.Tests
{
    /// <summary>
    /// Regression tests for Ngo1 adapter registration and the static-reset ordering it depends on.
    /// Background: multiplayer.tools #775 replaced the order-independent NetworkManager lookup with a
    /// registration that relied solely on the one-shot NetworkManager.OnInstantiated event, and the
    /// adapter/metric statics were reset at AfterAssembliesLoaded — the same phase the adapter subscribes.
    /// The combination left the adapter unregistered (or its metric subscription wiped) depending on when
    /// the NetworkManager was created relative to InitializeAdapter.
    /// </summary>
    internal class Ngo1AdapterRegistrationTests
    {
        GameObject m_NetworkGameObject;

        [SetUp]
        public void SetUp()
        {
            // Start hermetic: drop any adapter, poll, and leftover OnInstantiated subscription from a
            // previous run or from play-mode startup, so registration can't happen via a stale event
            // subscription and mask what each test is actually exercising. This is the same reset the
            // engine runs at SubsystemRegistration on every play-mode entry.
            Ngo1AdapterInitializer.ResetStaticsOnLoad();
            RemoveAllAdapters();
        }

        [TearDown]
        public void TearDown()
        {
            if (m_NetworkGameObject != null)
            {
                if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
                {
                    NetworkManager.Singleton.Shutdown();
                }
                // Destroy first so the initializer's OnDestroying handler (if still subscribed) can deinit
                // the adapter, then reset the initializer to a clean state for the next test.
                Object.DestroyImmediate(m_NetworkGameObject);
                m_NetworkGameObject = null;
            }
            Ngo1AdapterInitializer.ResetStaticsOnLoad();
            RemoveAllAdapters();
        }

        /// <summary>
        /// The core regression. When a NetworkManager already exists at the moment InitializeAdapter runs
        /// (e.g. instantiated by a multiplayer-session bootstrap before the AfterAssembliesLoaded hook), its
        /// one-shot OnInstantiated has already fired in Awake. Registration must still succeed via the
        /// Singleton poll fallback (same semantics as the pre-2.1 path: it completes once a session is
        /// running, i.e. NetworkTickSystem exists). Before the fix this asserted false — no adapter was
        /// created.
        /// </summary>
        [UnityTest]
        public IEnumerator RegistersAdapter_WhenNetworkManagerExistsBeforeInitialize()
        {
            Assert.IsFalse(HasNgo1Adapter(), "A Ngo1Adapter already existed before the test started.");

            // NetworkManager (and its one-shot OnInstantiated) comes up BEFORE InitializeAdapter, with no
            // subscriber present to catch the event.
            StartHost();
            yield return null;

            Ngo1AdapterInitializer.InitializeAdapter();
            yield return null;

            Assert.IsTrue(
                HasNgo1Adapter(),
                "Ngo1Adapter was not registered for a NetworkManager that already existed before InitializeAdapter ran.");
        }

        /// <summary>
        /// Complements the core test (NetworkManager created after InitializeAdapter): the poll fallback
        /// plus the OnInstantiated event must register exactly one adapter, never two — OnInstantiated
        /// cancels the poll.
        /// </summary>
        [UnityTest]
        public IEnumerator RegistersExactlyOneAdapter_WhenNetworkManagerCreatedAfterInitialize()
        {
            Ngo1AdapterInitializer.InitializeAdapter();
            StartHost();
            yield return null;

            Assert.AreEqual(
                1,
                CountNgo1Adapters(),
                "Expected exactly one Ngo1Adapter (lookup and OnInstantiated must not double-register).");
        }

        // Reflects on ResetStaticsOnLoad, which is editor-only (#if UNITY_EDITOR); this assembly has empty
        // includePlatforms so it also builds for players, where those methods don't exist. Editor-only.
#if UNITY_EDITOR
        /// <summary>
        /// Guardrail for the metrics/bandwidth regression. The bug was a phase race the runtime can't easily
        /// re-trigger, so we assert the invariant directly: the static-event resets run at the earliest phase
        /// (SubsystemRegistration) so they always precede the adapter's later-phase subscription and can't wipe
        /// it. If someone moves a reset back to AfterAssembliesLoaded, this fails.
        /// </summary>
        [Test]
        public void StaticResets_RunAtEarlierPhaseThanAdapterSubscription()
        {
            AssertLoadType(
                typeof(Ngo1AdapterInitializer),
                "ResetStaticsOnLoad",
                RuntimeInitializeLoadType.SubsystemRegistration);

            AssertLoadType(
                "Unity.Multiplayer.Tools.MetricEvents.MetricEventPublisher, Unity.Multiplayer.Tools.MetricEvents",
                "ResetStaticsOnLoad",
                RuntimeInitializeLoadType.SubsystemRegistration);

            AssertLoadType(
                typeof(NetworkAdapters),
                "ResetStaticsOnLoad",
                RuntimeInitializeLoadType.SubsystemRegistration);

            // The adapter registers/subscribes here; it must run in a strictly later phase than the resets.
            AssertLoadType(
                typeof(Ngo1AdapterInitializer),
                "InitializeAdapter",
                RuntimeInitializeLoadType.AfterAssembliesLoaded);
        }

        static void AssertLoadType(string assemblyQualifiedTypeName, string methodName, RuntimeInitializeLoadType expected)
        {
            var type = System.Type.GetType(assemblyQualifiedTypeName);
            Assert.IsNotNull(type, $"Could not resolve type '{assemblyQualifiedTypeName}'.");
            AssertLoadType(type, methodName, expected);
        }

        static void AssertLoadType(System.Type type, string methodName, RuntimeInitializeLoadType expected)
        {
            var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Could not find static method '{methodName}' on '{type.FullName}'.");

            var attribute = method.GetCustomAttribute<RuntimeInitializeOnLoadMethodAttribute>();
            Assert.IsNotNull(attribute, $"'{type.FullName}.{methodName}' is missing [RuntimeInitializeOnLoadMethod].");
            Assert.AreEqual(
                expected,
                attribute.loadType,
                $"'{type.FullName}.{methodName}' must run at {expected} so the reset/subscribe ordering stays deterministic.");
        }
#endif

        static bool HasNgo1Adapter() => NetworkAdapters.Adapters.Any(adapter => adapter is Ngo1Adapter);

        static int CountNgo1Adapters() => NetworkAdapters.Adapters.Count(adapter => adapter is Ngo1Adapter);

        static void RemoveAllAdapters()
        {
            foreach (var adapter in NetworkAdapters.Adapters.ToList())
            {
                NetworkAdapters.RemoveAdapter(adapter);
            }
        }

        void StartHost()
        {
            m_NetworkGameObject = new GameObject(nameof(Ngo1AdapterRegistrationTests));
            var networkManager = m_NetworkGameObject.AddComponent<NetworkManager>();
            var transport = m_NetworkGameObject.AddComponent<UnityTransport>();
            networkManager.NetworkConfig = new NetworkConfig
            {
                NetworkTransport = transport,
            };
            networkManager.StartHost();
        }
    }
}
