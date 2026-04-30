using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetVis.Configuration
{
    [Serializable]
    class MeshShadingGradient
    {
        [AllowNull]
        public Gradient Gradient = new();

        public MeshShadingGradientPreset Preset = MeshShadingGradientPreset.None;

        public bool UseGradient => Preset == MeshShadingGradientPreset.None;
    }
}
