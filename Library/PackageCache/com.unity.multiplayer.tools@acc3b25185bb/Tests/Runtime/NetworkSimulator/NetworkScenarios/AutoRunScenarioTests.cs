using System.Threading.Tasks;
using NUnit.Framework;
using Unity.Multiplayer.Tools.NetworkSimulator.Runtime;
using Unity.Multiplayer.Tools.NetworkSimulator.Runtime.BuiltInScenarios;
using UnityEngine;

namespace Unity.Multiplayer.Tools.Tests.NetworkSimulator
{
    class AutoRunScenarioTests
    {
        const int ChangeIntervalMilliseconds = 10;

        Tools.NetworkSimulator.Runtime.NetworkSimulator m_NetworkSimulator;
        RandomConnectionsSwap m_Scenario;
        ConnectionsCycle m_SecondScenario;

        [SetUp]
        public void SetUp()
        {
            m_Scenario = new RandomConnectionsSwap();
            m_Scenario.Configurations.Clear();
            m_Scenario.Configurations.Add(new RandomConnectionsSwap.Configuration
            {
                ConnectionPreset = NetworkSimulatorPresets.Mobile5G,
            });
            m_Scenario.ChangeIntervalMilliseconds = ChangeIntervalMilliseconds;

            m_SecondScenario = new ConnectionsCycle();
            m_SecondScenario.Configurations.Clear();
            m_SecondScenario.Configurations.Add(new ConnectionsCycle.Configuration
            {
                ConnectionPreset = NetworkSimulatorPresets.Mobile2G,
                ChangeIntervalMilliseconds = ChangeIntervalMilliseconds,
            });
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(m_NetworkSimulator.gameObject);
        }

        [Test]
        public async Task Scenario_WhenStartedWithAutoRunFalse_DoesNotStartScenario()
        {
            m_NetworkSimulator = ScenariosHelper.CreateNetworkSimulator();
            m_NetworkSimulator.ConnectionPreset = NetworkSimulatorPresets.None;
            m_NetworkSimulator.AutoRunScenario = false;
            m_NetworkSimulator.Scenario = m_Scenario;

            await Task.Delay(ChangeIntervalMilliseconds * 2);

            Assert.AreEqual(NetworkSimulatorPresets.None, m_NetworkSimulator.ConnectionPreset);
        }

        [Test]
        public async Task Scenario_WhenStartedWithAutoRunTrue_StartsScenario()
        {
            m_NetworkSimulator = ScenariosHelper.CreateNetworkSimulator();
            m_NetworkSimulator.ConnectionPreset = NetworkSimulatorPresets.None;
            m_NetworkSimulator.AutoRunScenario = true;
            m_NetworkSimulator.Scenario = m_Scenario;

            await Task.Delay(ChangeIntervalMilliseconds * 2);

            Assert.AreEqual(NetworkSimulatorPresets.Mobile5G, m_NetworkSimulator.ConnectionPreset);
        }

        [Test]
        public async Task Scenario_WhenAutoRunFalseAndNewScenarioIsSelected_DoesNotStartScenario()
        {
            m_NetworkSimulator = ScenariosHelper.CreateNetworkSimulator();
            m_NetworkSimulator.ConnectionPreset = NetworkSimulatorPresets.None;
            m_NetworkSimulator.AutoRunScenario = false;
            m_NetworkSimulator.Scenario = m_Scenario;

            await Task.Delay(ChangeIntervalMilliseconds * 2);

            Assert.AreEqual(NetworkSimulatorPresets.None, m_NetworkSimulator.ConnectionPreset);

            m_NetworkSimulator.Scenario = m_SecondScenario;

            await Task.Delay(ChangeIntervalMilliseconds * 2);

            Assert.AreEqual(NetworkSimulatorPresets.None, m_NetworkSimulator.ConnectionPreset);
        }

        [Test]
        public async Task Scenario_WhenAutoRunTrueAndNewScenarioIsSelected_StartsScenario()
        {
            m_NetworkSimulator = ScenariosHelper.CreateNetworkSimulator();
            m_NetworkSimulator.ConnectionPreset = NetworkSimulatorPresets.None;
            m_NetworkSimulator.AutoRunScenario = true;
            m_NetworkSimulator.Scenario = m_Scenario;

            await Task.Delay(ChangeIntervalMilliseconds * 2);

            Assert.AreEqual(NetworkSimulatorPresets.Mobile5G, m_NetworkSimulator.ConnectionPreset);

            m_NetworkSimulator.Scenario = m_SecondScenario;

            await Task.Delay(ChangeIntervalMilliseconds * 2);

            Assert.AreEqual(NetworkSimulatorPresets.Mobile2G, m_NetworkSimulator.ConnectionPreset);
        }

        [Test]
        public async Task Scenario_WhenAutoRunFalseAndScenarioIsResumed_StartsScenario()
        {
            m_NetworkSimulator = ScenariosHelper.CreateNetworkSimulator();
            m_NetworkSimulator.ConnectionPreset = NetworkSimulatorPresets.None;
            m_NetworkSimulator.AutoRunScenario = false;
            m_NetworkSimulator.Scenario = m_Scenario;

            await Task.Delay(ChangeIntervalMilliseconds * 2);

            Assert.AreEqual(NetworkSimulatorPresets.None, m_NetworkSimulator.ConnectionPreset);

            m_Scenario.IsPaused = false;

            await Task.Delay(ChangeIntervalMilliseconds * 2);

            Assert.AreEqual(NetworkSimulatorPresets.Mobile5G, m_NetworkSimulator.ConnectionPreset);
        }
    }
}
