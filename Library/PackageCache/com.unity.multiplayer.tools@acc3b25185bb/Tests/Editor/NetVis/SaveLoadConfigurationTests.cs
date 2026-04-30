using NUnit.Framework;
using Unity.Multiplayer.Tools.NetVis.Configuration;
using Unity.Multiplayer.Tools.NetVis.Editor.UI;

namespace Unity.Multiplayer.Tools.NetVis.Tests.Editor
{
    /// <summary>
    /// Tests to ensure that the NetVisConfiguration can be correctly
    /// saved and loaded from editor prefs
    /// </summary>
    [TestFixture]
    static class SaveLoadConfigurationTests
    {
        /// Use a different key than the default one that is used to save/load the configuration
        /// from editor prefs, so that running the tests does not stomp the local configuration,
        /// which would be inconvenient for developers working on NetVis.
        const string k_EditorPrefsKey = nameof(NetVisConfiguration) + "Test";

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
            SaveLoadEditorPrefs.Save(inputConfiguration, k_EditorPrefsKey);
            var outputConfiguration = SaveLoadEditorPrefs.Load<NetVisConfiguration>(k_EditorPrefsKey);

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
