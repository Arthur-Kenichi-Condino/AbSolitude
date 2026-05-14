using JetBrains.Annotations;
using Unity.Multiplayer.Tools.Adapters;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.DependencyInjection;
using Unity.Multiplayer.Tools.NetVis.Configuration;
using UnityEditor;

namespace Unity.Multiplayer.Tools.NetVis.Editor.UI
{
    class PresentationContext : EditorOnlyContext
    {
        /// <remarks>
        /// The singleton is needed so that we can access information
        /// (like the <see cref="ConfigurationWithEvents"/>) from static
        /// methods like the <see cref="NetVisToolbarOverlay"/>
        /// </remarks>>
        public static PresentationContext Instance { get; private set; }

        [NotNull]
        public NetVisConfigurationWithEvents ConfigurationWithEvents { get; }

        [NotNull]
        IReadonlyBandwidthStats BandwidthStats { get; }

        [NotNull]
        IGetConnectedClients ConnectedClientsRepository { get; }

        public static PresentationContext InitializeInstance(
            [NotNull] NetVisConfigurationWithEvents configurationWithEvents,
            [NotNull] IReadonlyBandwidthStats bandwidthStats,
            [NotNull] IGetConnectedClients connectedClients)
        {
            return Instance ??= new PresentationContext(
                configurationWithEvents,
                bandwidthStats,
                connectedClients);
        }

        PresentationContext(
            [NotNull] NetVisConfigurationWithEvents configWithEvents,
            [NotNull] IReadonlyBandwidthStats bandwidthStats,
            [NotNull] IGetConnectedClients connectedClients)
        {
            ConfigurationWithEvents = configWithEvents;
            BandwidthStats = bandwidthStats;
            ConnectedClientsRepository = connectedClients;

            var configLoaded = SaveLoadEditorPrefs.Load<NetVisConfiguration>();
            if (configLoaded != null)
            {
                ConfigurationWithEvents.Configuration = configLoaded;
            }

            ConfigurationWithEvents.ConfigurationChanged += configuration =>
            {
                SaveLoadEditorPrefs.Save(configuration);
            };
        }

        protected override void Setup()
        {
            Context.Current
                .AddSingleton(ConfigurationWithEvents)
                .AddSingleton(BandwidthStats)
                .AddSingleton(ConnectedClientsRepository);
        }

#if UNITY_MP_TOOLS_DEV
        [MenuItem("Window/Multiplayer/Multiplayer Tools Dev/Reset NetVis Configuration")]
        public static void ResetNetVisConfiguration()
        {
            Instance.ConfigurationWithEvents.Configuration.Settings.Ownership.ResetCustomColors();
            Instance.ConfigurationWithEvents.Configuration = new NetVisConfiguration();
        }
#endif
    }
}
