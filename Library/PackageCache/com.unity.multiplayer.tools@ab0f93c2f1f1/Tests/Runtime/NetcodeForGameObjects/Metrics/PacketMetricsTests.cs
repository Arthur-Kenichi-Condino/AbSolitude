#if COM_UNITY_NETCODE_FOR_GAMEOBJECTS_V2_4_X
using System.Collections;
using NUnit.Framework;
using Unity.Collections;
using Unity.Multiplayer.Tools.MetricTypes;
using Unity.Netcode;
using UnityEngine.TestTools;

namespace Unity.Multiplayer.Tools.GameObjects.Tests
{
    internal class PacketMetricsTests : SingleClientMetricTestBase
    {
        [UnityTest]
        public IEnumerator TrackPacketSentMetric()
        {
            var waitForMetricValues = new WaitForCounterMetricValue(ServerMetrics.Dispatcher, NetworkMetricTypes.PacketsSent, metric => metric > 0);

            using (var writer = new FastBufferWriter(sizeof(uint), Allocator.Temp))
            {
                writer.WriteValueSafe(1337);
                Server.CustomMessagingManager.SendUnnamedMessageToAll(writer);
            }

            yield return waitForMetricValues.WaitForMetricsReceived();

            var totalPacketCount = waitForMetricValues.AssertMetricValueHaveBeenFound();
            Assert.That(totalPacketCount, Is.InRange(1, 4));
        }

        [UnityTest]
        public IEnumerator TrackPacketReceivedMetric()
        {
            var waitForMetricValues = new WaitForCounterMetricValue(ClientMetrics.Dispatcher, NetworkMetricTypes.PacketsReceived, metric => metric > 0);

            using (var writer = new FastBufferWriter(sizeof(uint), Allocator.Temp))
            {
                writer.WriteValueSafe(1337);
                Server.CustomMessagingManager.SendUnnamedMessageToAll(writer);
            }

            yield return waitForMetricValues.WaitForMetricsReceived();

            var totalPacketCount = waitForMetricValues.AssertMetricValueHaveBeenFound();
            Assert.That(totalPacketCount, Is.InRange(1, 4));
        }
    }
}
#endif
