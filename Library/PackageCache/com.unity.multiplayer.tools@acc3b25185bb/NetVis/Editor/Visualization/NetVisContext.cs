using Unity.Multiplayer.Tools.Adapters;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.NetVis.Configuration;

namespace Unity.Multiplayer.Tools.NetVis.Editor.Visualization
{
    /// <summary>
    /// Main NetVis entry point. Maps the dependencies for this and children contexts.
    /// This class responsibility is to only handle Data and Systems bindings,
    /// any extra logic should be handled as a separate Systems.
    /// </summary>
    class NetVisContext : SetupHandler
    {
        // Configuration
        public NetVisConfigurationWithEvents ConfigurationWithEvents { get; }
        NetVisConfiguration Configuration => ConfigurationWithEvents.Configuration;

        // Feedback to the UI
        public IGetConnectedClients ConnectedClients => NetVisDataStore;
        public BandwidthStats BandwidthStats { get; }

        // Internal systems
        IRuntimeUpdater RuntimeUpdater { get; }
        NetVisDataStore NetVisDataStore { get; }
        VisualizationSystem m_VisualizationSystem;

        public static NetVisContext InitializeInstance(IRuntimeUpdater runtimeUpdater)
        {
            return new NetVisContext(runtimeUpdater);
        }

        NetVisContext(IRuntimeUpdater runtimeUpdater)
        {
            DebugUtil.TraceMethodName();
            RuntimeUpdater = runtimeUpdater;
            ConfigurationWithEvents = new NetVisConfigurationWithEvents(new NetVisConfiguration());
            BandwidthStats = new BandwidthStats();
            NetVisDataStore = new NetVisDataStore(Configuration, BandwidthStats);
        }

        protected override void Setup()
        {
            DebugUtil.TraceMethodName();
            RuntimeUpdater.OnStart += OnStart;
            RuntimeUpdater.OnDestroyed += OnDestroyed;
        }

        protected override void Teardown()
        {
            DebugUtil.TraceMethodName();
            RuntimeUpdater.OnStart -= OnStart;
            RuntimeUpdater.OnDestroyed -= OnDestroyed;
        }

        void OnStart()
        {
            DebugUtil.TraceMethodName();
            m_VisualizationSystem = new VisualizationSystem(
                ConfigurationWithEvents,
                NetVisDataStore,
                RuntimeUpdater);
        }

        void OnDestroyed()
        {
            DebugUtil.TraceMethodName();

            m_VisualizationSystem?.Dispose();
            m_VisualizationSystem = null;
        }
    }
}
