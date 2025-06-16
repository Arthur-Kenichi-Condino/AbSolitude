using UnityEngine;
using Unity.Multiplayer.Tools.Adapters;
using Unity.Multiplayer.Tools.Common.Visualization;

namespace Unity.Multiplayer.Tools.NetVis.Configuration
{
    class OwnershipSettings
    {
        public bool MeshShadingEnabled { get; set; } = true;
        public bool TextOverlayEnabled { get; set; } = true;

        public Color ServerHostColor { get; } = CategoricalColorPalette.GetColor(0);

        // We use a dictionary here, for two reasons
        // 1. Connected clients can come out of order
        // 2. We want default settings, but there's no guarantee clientIds use sequential integers
        internal ReadonlyClientColorDictionary ClientColors { get; } = new();
    }

    /// <remarks>
    /// Temporary, readonly stand-in for a dictionary of ClientId -> Color, until we reintroduce customization
    /// of client colors in a future release. 2023-04-06
    /// </remarks>>
    class ReadonlyClientColorDictionary
    {
        public bool TryGetValue(ClientId clientId, out Color color) =>
            CategoricalColorPalette.TryGetColor((int)clientId, out color);
    }
}