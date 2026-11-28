using System;
using UnityEngine;
using Unity.Multiplayer.Tools.Adapters;
using Unity.Multiplayer.Tools.Common.Visualization;

namespace Unity.Multiplayer.Tools.NetVis.Configuration
{
    class OwnershipSettings
    {
        public bool MeshShadingEnabled { get; set; } = true;
        public bool TextOverlayEnabled { get; set; } = true;
        public event Action ColorsChanged;

#if UNITY_EDITOR
        public OwnershipSettings()
        {
            CustomColorSettings.DataChanged += () => ColorsChanged?.Invoke();
        }
#endif

        public Color ServerHostColor
        {
            get
            {
#if UNITY_EDITOR
                if (CustomColorSettings.HasColor(0))
                {
                    return CustomColorSettings.GetColor(0);
                }
#endif

                return CategoricalColorPalette.GetColor(0);
            }
        }

        internal Color GetClientColor(ClientId clientId)
        {
#if UNITY_EDITOR
            if (CustomColorSettings.HasColor((int)clientId))
            {
                return CustomColorSettings.GetColor((int)clientId);
            }
#endif

            return CategoricalColorPalette.GetColor((int)clientId);
        }

        internal void SetCustomColor(ClientId clientId, Color color)
        {
#if UNITY_EDITOR
            CustomColorSettings.SetColor((int)clientId, color);
            ColorsChanged?.Invoke();
#endif
        }

        internal void ResetCustomColor(ClientId clientId)
        {
#if UNITY_EDITOR
            CustomColorSettings.RemoveColor((int)clientId);
            ColorsChanged?.Invoke();
#endif
        }

        internal void ResetCustomColors()
        {
#if UNITY_EDITOR
            CustomColorSettings.ClearColors();
            ColorsChanged?.Invoke();
#endif
        }
    }
}