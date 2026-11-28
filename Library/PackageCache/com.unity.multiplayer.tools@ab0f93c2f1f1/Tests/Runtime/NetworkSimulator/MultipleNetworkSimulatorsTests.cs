using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.Multiplayer.Tools.Adapters;
using Unity.Multiplayer.Tools.NetworkSimulator.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.Multiplayer.Tools.Tests.NetworkSimulator
{
    internal class MultipleNetworkSimulatorsTests
    {
        /// Deliberately count-agnostic, so a simulator leaked by another fixture cannot fail the match.
        static readonly Regex k_MultipleSimulatorsWarning = new("enabled NetworkSimulator components");

        MockNetworkParametersAdapter m_Adapter;

        [SetUp]
        public void Setup()
        {
            DestroyAllSimulators();
            m_Adapter = new MockNetworkParametersAdapter();
            NetworkAdapters.AddAdapter(m_Adapter);
        }

        [TearDown]
        public void TearDown()
        {
            DestroyAllSimulators();
            NetworkAdapters.RemoveAdapter(m_Adapter);
            m_Adapter = null;
        }

        [Test]
        public void EnablingSecondSimulator_LogsWarning()
        {
            CreateSimulator("First Simulator", packetDelayMs: 100);

            LogAssert.Expect(LogType.Warning, k_MultipleSimulatorsWarning);
            CreateSimulator("Second Simulator", packetDelayMs: 50);
        }

        [Test]
        public void DisablingSecondSimulator_RestoresParametersOfTheSimulatorStillEnabled()
        {
            CreateSimulator("First Simulator", packetDelayMs: 100);

            LogAssert.Expect(LogType.Warning, k_MultipleSimulatorsWarning);
            var second = CreateSimulator("Second Simulator", packetDelayMs: 50);
            Assert.That(m_Adapter.NetworkParameters.PacketDelayMilliseconds, Is.EqualTo(50));

            second.enabled = false;

            Assert.That(m_Adapter.NetworkParameters.PacketDelayMilliseconds, Is.EqualTo(100));
        }

        [Test]
        public void DisablingOnlySimulator_ClearsParameters()
        {
            var simulator = CreateSimulator("Only Simulator", packetDelayMs: 100);
            Assert.That(m_Adapter.NetworkParameters.PacketDelayMilliseconds, Is.EqualTo(100));

            simulator.enabled = false;

            Assert.That(m_Adapter.NetworkParameters.PacketDelayMilliseconds, Is.EqualTo(0));
        }

        static Tools.NetworkSimulator.Runtime.NetworkSimulator CreateSimulator(string name, int packetDelayMs)
        {
            var simulator = new GameObject(name)
                .AddComponent<Tools.NetworkSimulator.Runtime.NetworkSimulator>();
            simulator.ConnectionPreset = NetworkSimulatorPreset.Create(name, packetDelayMs: packetDelayMs);
            return simulator;
        }

        static void DestroyAllSimulators()
        {
#if UNITY_6000_4_OR_NEWER
            var simulators = Object.FindObjectsByType<Tools.NetworkSimulator.Runtime.NetworkSimulator>(
                FindObjectsInactive.Include);
#else
            var simulators = Object.FindObjectsByType<Tools.NetworkSimulator.Runtime.NetworkSimulator>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
#endif
            foreach (var simulator in simulators)
            {
                Object.DestroyImmediate(simulator.gameObject);
            }
        }

        class MockNetworkParametersAdapter : INetworkAdapter, IHandleNetworkParameters
        {
            public AdapterMetadata Metadata => default;

            public NetworkParameters NetworkParameters { get; set; } = new();

            public T GetComponent<T>() where T : class, IAdapterComponent => this as T;
        }
    }
}
