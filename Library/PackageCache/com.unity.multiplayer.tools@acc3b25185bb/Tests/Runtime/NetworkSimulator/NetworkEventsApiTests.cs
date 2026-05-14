using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Unity.Multiplayer.Tools.Adapters;
using Unity.Multiplayer.Tools.NetworkSimulator.Runtime;
using UnityEngine;

namespace Unity.Multiplayer.Tools.Tests.NetworkSimulator
{
    internal class NetworkEventsApiTests
    {
        Tools.NetworkSimulator.Runtime.NetworkSimulator m_NetworkSimulator;
        MockTransportApi m_NetworkTransportApi;

        NetworkEventsApi m_NetworkEventsApi;

        [SetUp]
        public void SetUp()
        {
            var gameObject = new GameObject();

            m_NetworkSimulator = gameObject.AddComponent<Tools.NetworkSimulator.Runtime.NetworkSimulator>();

            m_NetworkTransportApi = new MockTransportApi();

            m_NetworkEventsApi = new NetworkEventsApi(m_NetworkSimulator, m_NetworkTransportApi);
        }

        [TestCase(false, false)]
        [TestCase(true, true)]
        public void IsAvailable_Always_ReturnsUnderlyingApiValue(bool apiValue, bool expectedValue)
        {
            m_NetworkTransportApi.IsAvailable = apiValue;

            var actualValue = m_NetworkEventsApi.IsAvailable;

            Assert.AreEqual(expectedValue, actualValue);
        }

        [TestCase(false, false)]
        [TestCase(true, true)]
        public void IsConnected_Always_ReturnsUnderlyingApiValue(bool apiValue, bool expectedValue)
        {
            m_NetworkTransportApi.IsConnected = apiValue;

            var actualValue = m_NetworkEventsApi.IsConnected;

            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void Disconnect_WhenSimulatorIsDisabled_DoesNothing()
        {
            m_NetworkSimulator.enabled = false;

            m_NetworkEventsApi.Disconnect();

            Assert.AreEqual(0, m_NetworkTransportApi.SimulateDisconnectCallCount);
            Assert.AreEqual(0, m_NetworkTransportApi.SimulateReconnectCallCount);
            Assert.AreEqual(0, m_NetworkTransportApi.UpdateNetworkParametersCallCount);
        }

        [Test]
        public void Disconnect_WhenSimulatorIsEnabled_CallsSimulateDisconnect()
        {
            m_NetworkSimulator.enabled = true;

            m_NetworkEventsApi.Disconnect();

            Assert.AreEqual(1, m_NetworkTransportApi.SimulateDisconnectCallCount);
            Assert.AreEqual(0, m_NetworkTransportApi.SimulateReconnectCallCount);
            Assert.AreEqual(0, m_NetworkTransportApi.UpdateNetworkParametersCallCount);
        }

        [Test]
        public void Reconnect_WhenSimulatorIsDisabled_DoesNothing()
        {
            m_NetworkSimulator.enabled = false;

            m_NetworkEventsApi.Reconnect();

            Assert.AreEqual(0, m_NetworkTransportApi.SimulateDisconnectCallCount);
            Assert.AreEqual(0, m_NetworkTransportApi.SimulateReconnectCallCount);
            Assert.AreEqual(0, m_NetworkTransportApi.UpdateNetworkParametersCallCount);
        }

        [Test]
        public void Reconnect_WhenSimulatorIsEnabled_CallsSimulateReconnect()
        {
            m_NetworkSimulator.enabled = true;

            m_NetworkEventsApi.Reconnect();

            Assert.AreEqual(0, m_NetworkTransportApi.SimulateDisconnectCallCount);
            Assert.AreEqual(1, m_NetworkTransportApi.SimulateReconnectCallCount);
            Assert.AreEqual(0, m_NetworkTransportApi.UpdateNetworkParametersCallCount);
        }

        [Test]
        public void TriggerLagSpike_WhenSimulatorIsDisabled_DoesNothing()
        {
            m_NetworkSimulator.enabled = false;

            m_NetworkEventsApi.TriggerLagSpike(TimeSpan.MaxValue);

            Assert.AreEqual(0, m_NetworkTransportApi.SimulateDisconnectCallCount);
            Assert.AreEqual(0, m_NetworkTransportApi.SimulateReconnectCallCount);
            Assert.AreEqual(0, m_NetworkTransportApi.UpdateNetworkParametersCallCount);
        }

        [Test]
        public async Task TriggerLagSpike_WhenSimulatorIsEnabled_DisconnectsWaitsAndReconnects()
        {
            m_NetworkSimulator.enabled = true;

            await m_NetworkEventsApi.TriggerLagSpikeAsync(TimeSpan.FromMilliseconds(10));

            Assert.AreEqual(1, m_NetworkTransportApi.SimulateDisconnectCallCount);
            Assert.AreEqual(1, m_NetworkTransportApi.SimulateReconnectCallCount);
            Assert.AreEqual(0, m_NetworkTransportApi.UpdateNetworkParametersCallCount);
        }

        [Test]
        public void ChangeConnectionPreset_WhenSimulatorIsDisabled_DoesNothing()
        {
            m_NetworkSimulator.enabled = false;
            m_NetworkSimulator.ConnectionPreset = NetworkSimulatorPresets.None;

            m_NetworkEventsApi.ChangeConnectionPreset(NetworkSimulatorPresets.HomeBroadband);

            Assert.AreEqual(NetworkSimulatorPresets.None.Name, m_NetworkSimulator.ConnectionPreset.Name);
        }

        [Test]
        public void ChangeConnectionPreset_WhenSimulatorIsEnabled_SetsSimulatorPreset()
        {
            m_NetworkSimulator.enabled = true;
            m_NetworkSimulator.ConnectionPreset = NetworkSimulatorPresets.None;

            m_NetworkEventsApi.ChangeConnectionPreset(NetworkSimulatorPresets.HomeBroadband);

            Assert.AreEqual(NetworkSimulatorPresets.HomeBroadband.Name, m_NetworkSimulator.ConnectionPreset.Name);
        }

        class MockTransportApi : INetworkTransportApi
        {
            public bool IsAvailable { get; set; }

            public bool IsConnected { get; set; }

            public int SimulateDisconnectCallCount { get; private set; } = 0;

            public int SimulateReconnectCallCount { get; private set; } = 0;

            public int UpdateNetworkParametersCallCount { get; private set; } = 0;

            public void SimulateDisconnect()
            {
                SimulateDisconnectCallCount++;
            }

            public void SimulateReconnect()
            {
                SimulateReconnectCallCount++;
            }

            public void UpdateNetworkParameters(NetworkParameters networkParameters)
            {
                UpdateNetworkParametersCallCount++;
            }
        }
    }
}
