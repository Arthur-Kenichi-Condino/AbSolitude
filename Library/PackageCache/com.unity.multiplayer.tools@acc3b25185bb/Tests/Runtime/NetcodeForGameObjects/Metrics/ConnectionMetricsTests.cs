#if COM_UNITY_NETCODE_FOR_GAMEOBJECTS_V2_4_X

using System.Collections;
using NUnit.Framework;
using Unity.Multiplayer.Tools.MetricTypes;
using Unity.Netcode;
using Unity.Netcode.TestHelpers.Runtime;
using UnityEngine.TestTools;

namespace Unity.Multiplayer.Tools.GameObjects.Tests
{
    [TestFixture(ClientCount.OneClient, HostOrServer.Host)]
    [TestFixture(ClientCount.TwoClients, HostOrServer.Host)]
    [TestFixture(ClientCount.OneClient, HostOrServer.Server)]
    [TestFixture(ClientCount.TwoClients, HostOrServer.Server)]
    internal class ConnectionMetricsTests : NetcodeIntegrationTest
    {
        protected override int NumberOfClients => m_ClientCount;

        private int m_ClientCount;

        public enum ClientCount
        {
            OneClient = 1,
            TwoClients,
        }

        public ConnectionMetricsTests(ClientCount clientCount, HostOrServer hostOrServer)
            : base(hostOrServer)
        {
            m_ClientCount = (int)clientCount;
        }

        private int GetClientCountForFixture()
        {
            return m_ClientCount + ((m_UseHost) ? 1 : 0);
        }

        [UnityTest]
        public IEnumerator UpdateConnectionCountOnServer()
        {
            var waitForGaugeValues = new WaitForGaugeMetricValues((m_ServerNetworkManager.NetworkMetrics as NetworkMetrics).Dispatcher, NetworkMetricTypes.ConnectedClients);

            yield return waitForGaugeValues.WaitForMetricsReceived();

            var value = waitForGaugeValues.AssertMetricValueHaveBeenFound();
            Assert.AreEqual(GetClientCountForFixture(), value);
        }

        [UnityTest]
        public IEnumerator UpdateConnectionCountOnClient()
        {
            foreach (var clientNetworkManager in m_ClientNetworkManagers)
            {
                var waitForGaugeValues = new WaitForGaugeMetricValues((clientNetworkManager.NetworkMetrics as NetworkMetrics).Dispatcher, NetworkMetricTypes.ConnectedClients);

                yield return waitForGaugeValues.WaitForMetricsReceived();

                var value = waitForGaugeValues.AssertMetricValueHaveBeenFound();
                Assert.AreEqual(1, value);
            }
        }
    }
}
#endif