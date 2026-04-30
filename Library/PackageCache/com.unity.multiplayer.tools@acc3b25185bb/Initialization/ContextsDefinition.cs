using System.Collections.Generic;
using Unity.Multiplayer.Tools.Common;

namespace Unity.Multiplayer.Tools.Context
{
    static class ContextsDefinition
    {
        internal static IContext[] Contexts { get; }

        static ContextsDefinition()
        {
            var contexts = new List<IContext>();

            IRuntimeUpdater runtimeUpdater = new RuntimeUpdater();

            InitializeNetVisContexts(runtimeUpdater, contexts);

            Contexts = contexts.ToArray();
        }

        static void InitializeNetVisContexts(IRuntimeUpdater runtimeUpdater, List<IContext> contexts)
        {
#if NETVIS_AVAILABLE && UNITY_EDITOR
            var netVisRuntimeContext = NetVis.Editor.Visualization.NetVisContext.InitializeInstance(runtimeUpdater);
            contexts.Add(netVisRuntimeContext);
            var netVisPresentationContext = NetVis.Editor.UI.PresentationContext.InitializeInstance(
                netVisRuntimeContext.ConfigurationWithEvents,
                netVisRuntimeContext.BandwidthStats,
                netVisRuntimeContext.ConnectedClients);
            contexts.Add(netVisPresentationContext);
#endif 
        }
    }
}
