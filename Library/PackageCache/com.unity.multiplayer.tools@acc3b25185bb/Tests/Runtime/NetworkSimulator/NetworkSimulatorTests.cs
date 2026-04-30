using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Multiplayer.Tools.NetworkSimulator.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.Multiplayer.Tools.Tests.NetworkSimulator
{
    internal class NetworkSimulatorTests
    {
        Tools.NetworkSimulator.Runtime.NetworkSimulator m_NetworkSimulator;

        [SetUp]
        public void Setup()
        {
            if (m_NetworkSimulator == null)
            {
                m_NetworkSimulator = new GameObject().AddComponent<Tools.NetworkSimulator.Runtime.NetworkSimulator>();
            }
        }

        [Test]
        public void SetConnectionPreset_GivenNullValue_DoesntThrowException()
        {
            m_NetworkSimulator.ConnectionPreset = null;
            Assert.IsNull(m_NetworkSimulator.ConnectionPreset);
        }

        [Test]
        public void SetConnectionPreset_GivenPresetReference_DoesntThrowException()
        {
            var preset = new NetworkSimulatorPreset();
            m_NetworkSimulator.ConnectionPreset = preset;
            Assert.AreEqual(preset, m_NetworkSimulator.ConnectionPreset);
        }

        [Test]
        public void SetConnectionPreset_TestingAllValidPresets()
        {
            foreach (var preset in ScenariosHelper.GetAllValidPresetsArray())
            {
                m_NetworkSimulator.ConnectionPreset = preset;
                Assert.AreEqual(preset, m_NetworkSimulator.ConnectionPreset);
            }
        }

        [Test]
        public void ConnectionPresets_SpecificPresetsExist()
        {
            var presets = ScenariosHelper.GetAllValidPresetsArray();

            Assert.IsTrue(presets.Any(nonePreset => nonePreset.Name == "None"));
            Assert.IsTrue(presets.Any(homeBroadbandPreset => homeBroadbandPreset.Name == "Home Broadband [WIFI, Cable, Console, PC]"
                                                                && homeBroadbandPreset.Description == "Typical of desktop and console platforms (and generally speaking most mobile players too)."
                                                                && homeBroadbandPreset.PacketDelayMs == 32
                                                                && homeBroadbandPreset.PacketJitterMs == 12
                                                                && homeBroadbandPreset.PacketLossPercent == 2
                                                                && homeBroadbandPreset.PacketLossInterval == 0));
        }

        [Test]
        public void SetConnectionPreset_CorrectCallbacksCalled()
        {
            var preset = new NetworkSimulatorPreset();
            var propertyChangedCallbackIsCalled = false;

            m_NetworkSimulator.m_PropertyChanged += (_, args) =>
            {
                Assert.AreEqual(nameof(m_NetworkSimulator.ConnectionPreset), args.PropertyName);
                propertyChangedCallbackIsCalled = true;
            };

            m_NetworkSimulator.ScenarioChangedEvent += (_) =>
            {
                Assert.Fail("ScenarioChangedEvent should not be called");
            };

            m_NetworkSimulator.ConnectionPreset = preset;
            Assert.AreEqual(preset, m_NetworkSimulator.ConnectionPreset);
            Assert.True(propertyChangedCallbackIsCalled);
        }

        [Test]
        public void SetScenarioSettings_GivenNullValue_DoesntThrowException()
        {
            m_NetworkSimulator.Scenario = null;
            Assert.IsNull(m_NetworkSimulator.Scenario);
        }

        [Test]
        public void SetScenarioSettings_GivenObject_DoesntThrowException()
        {
            var scenario = new MockNetworkScenario();
            m_NetworkSimulator.Scenario = scenario;
            Assert.AreEqual(scenario, m_NetworkSimulator.Scenario);
        }

        [Test]
        public void SetScenario_CorrectCallbacksCalled()
        {
            var scenario = new MockNetworkScenario();
            var propertyChangedCallbackIsCalled = false;
            var scenarioChangedCallbackIsCalled = false;

            m_NetworkSimulator.m_PropertyChanged += (_, args) =>
            {
                Assert.Contains(args.PropertyName, new List<string> { nameof(m_NetworkSimulator.Scenario), nameof(m_NetworkSimulator.ConnectionPreset) });
                propertyChangedCallbackIsCalled = true;
            };

            m_NetworkSimulator.ScenarioChangedEvent += (newScenario) =>
            {
                Assert.AreEqual(scenario, newScenario);
                scenarioChangedCallbackIsCalled = true;
            };

            m_NetworkSimulator.Scenario = scenario;
            Assert.AreEqual(scenario, m_NetworkSimulator.Scenario);
            Assert.True(propertyChangedCallbackIsCalled);
            Assert.True(scenarioChangedCallbackIsCalled);
        }

        [Test]
        public void GetIsAvailable_ReturnsFalseWithoutExceptionOrUnexpectedLog()
        {
            Assert.That(m_NetworkSimulator.IsAvailable, Is.False);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void GetIsConnected_ReturnsFalseWithoutExceptionOrUnexpectedLog()
        {
            Assert.That(m_NetworkSimulator.IsConnected, Is.False);
            LogAssert.NoUnexpectedReceived();
        }

        [TearDown]
        public void TearDown()
        {
            if (m_NetworkSimulator != null)
            {
                Object.DestroyImmediate(m_NetworkSimulator.gameObject);
                m_NetworkSimulator = null;
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
#if UNITY_6000_4_OR_NEWER
            var remainingObjects = Object.FindObjectsByType<Tools.NetworkSimulator.Runtime.NetworkSimulator>(FindObjectsInactive.Include);
#else
            var remainingObjects = Object.FindObjectsByType<Tools.NetworkSimulator.Runtime.NetworkSimulator>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#endif
            foreach (var networkSimulator in remainingObjects)
            {
                Object.DestroyImmediate(networkSimulator.gameObject);
            }
        }
    }

    class MockNetworkScenario : NetworkScenarioBehaviour
    {
        public override void Start(INetworkEventsApi networkEventsApi)
        {
        }

        protected override void Update(float deltaTime)
        {
        }
    }
}
