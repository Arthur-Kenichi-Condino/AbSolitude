using System;
using System.Linq;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetVis.Configuration
{
    static class MeshShadingGradientExtensions
    {
        public static Color Evaluate(this MeshShadingGradient meshShadingFill, float percentage)
        {
            percentage = Mathf.Clamp01(percentage);

            if (meshShadingFill.UseGradient)
            {
                return meshShadingFill.Gradient.Evaluate(percentage);
            }

            return meshShadingFill.Preset switch
            {
                MeshShadingGradientPreset.Viridis => MatplotlibColorMaps.GetViridis(percentage),
                MeshShadingGradientPreset.Plasma => MatplotlibColorMaps.GetPlasma(percentage),
                MeshShadingGradientPreset.Magma => MatplotlibColorMaps.GetMagma(percentage),
                MeshShadingGradientPreset.Inferno => MatplotlibColorMaps.GetInferno(percentage),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static Gradient ToGradient(this MeshShadingGradient meshShadingFill)
        {
            return meshShadingFill.UseGradient
                ? meshShadingFill.Gradient
                : meshShadingFill.Preset.ToGradient();
        }

        public static Gradient ToGradient(this MeshShadingGradientPreset preset)
        {
            var colors = preset switch
            {
                MeshShadingGradientPreset.Viridis => MatplotlibColorMaps.GenerateViridis(8),
                MeshShadingGradientPreset.Plasma => MatplotlibColorMaps.GeneratePlasma(8),
                MeshShadingGradientPreset.Magma => MatplotlibColorMaps.GenerateMagma(8),
                MeshShadingGradientPreset.Inferno => MatplotlibColorMaps.GenerateInferno(8),
                _ => throw new ArgumentOutOfRangeException()
            };

            return new()
            {
                colorKeys = colors
                    .Select((color, index) => new GradientColorKey(color, index / 8f))
                    .ToArray()
            };
        }
    }
}
