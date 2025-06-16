using System;

namespace Unity.Multiplayer.Tools.NetVis.Configuration
{
    /// <summary>
    /// A full set of instructions for what should be rendered by the NetVis system,
    /// including:
    /// <br/>
    /// 1. The <see cref="NetVisMetric"/> to display.
    /// <br/>
    /// 2. The <see cref="NetVisSettings"/> to be applied to the <see cref="NetVisMetric"/>,
    /// and the NetVis system.
    /// </summary>
    [Serializable]
    class NetVisConfiguration
    {
        /// <summary>
        /// The <see cref="NetVisMetric"/> to be displayed.
        /// </summary>
        public NetVisMetric Metric { get; set; }

        public bool MeshShadingEnabled => Metric switch
        {
            NetVisMetric.Bandwidth => Settings.Bandwidth.MeshShadingEnabled,
            NetVisMetric.Ownership => Settings.Ownership.MeshShadingEnabled,
            _ => false,
        };

        public bool TextOverlayEnabled => Metric switch
        {
            NetVisMetric.Bandwidth => Settings.Bandwidth.TextOverlayEnabled,
            NetVisMetric.Ownership => Settings.Ownership.TextOverlayEnabled,
            _ => false,
        };

        /// <summary>
        /// The <see cref="NetVisSettings"/> to be applied to the chosen <see cref="NetVisMetric"/>
        /// and <see cref="Configuration.Visualizations"/>.
        /// </summary>
        public NetVisSettings Settings { get; } = new();
    }
}
