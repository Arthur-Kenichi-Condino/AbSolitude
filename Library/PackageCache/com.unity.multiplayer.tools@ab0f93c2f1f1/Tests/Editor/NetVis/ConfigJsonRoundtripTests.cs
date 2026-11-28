using NUnit.Framework;
using Unity.Multiplayer.Tools.NetVis.Configuration;
using Unity.Multiplayer.Tools.NetVis.Editor.UI;

namespace Unity.Multiplayer.Tools.NetVis.Tests.Editor
{
    /// <summary>
    /// Tests to ensure that the NetVisConfiguration can be "round-tripped"
    /// or converted to and from JSON correctly, to ensure that it can be
    /// saved and loaded from editor prefs.
    /// </summary>
    [TestFixture]
    static class ConfigJsonRoundtripTests
    {
        [TestCase(NetVisMetric.Bandwidth, false, false)]
        [TestCase(NetVisMetric.Ownership, false, false)]

        [TestCase(NetVisMetric.Bandwidth, true, false)]
        [TestCase(NetVisMetric.Bandwidth, false, true)]
        [TestCase(NetVisMetric.Ownership, true, false)]
        [TestCase(NetVisMetric.Ownership, false, true)]

        [TestCase(NetVisMetric.Bandwidth, true, true)]
        [TestCase(NetVisMetric.Ownership, true, true)]
        public static void RoundTripConfigurationWithMetricAndVisualizations(
            NetVisMetric metric,
            bool meshShadingEnabled,
            bool textOverlayEnabled)
        {
            var inputConfiguration = new NetVisConfiguration
            {
                Metric = metric,
                Settings =
                {
                    Bandwidth =
                    {
                        MeshShadingEnabled = meshShadingEnabled,
                        TextOverlayEnabled = textOverlayEnabled,
                    },
                    Ownership =
                    {
                        MeshShadingEnabled = meshShadingEnabled,
                        TextOverlayEnabled = textOverlayEnabled,
                    }
                }
            };

            var inputJson = JsonSerialization.ToJson(inputConfiguration);
            var outputConfiguration = JsonSerialization.FromJson<NetVisConfiguration>(inputJson);

            Assert.That(outputConfiguration.Metric, Is.EqualTo(metric));
            Assert.That(outputConfiguration.Settings.Bandwidth.MeshShadingEnabled,
                Is.EqualTo(meshShadingEnabled));
            Assert.That(outputConfiguration.Settings.Bandwidth.TextOverlayEnabled,
                Is.EqualTo(textOverlayEnabled));
            Assert.That(outputConfiguration.Settings.Ownership.MeshShadingEnabled,
                Is.EqualTo(meshShadingEnabled));
            Assert.That(outputConfiguration.Settings.Ownership.TextOverlayEnabled,
                Is.EqualTo(textOverlayEnabled));
        }
    }
}
