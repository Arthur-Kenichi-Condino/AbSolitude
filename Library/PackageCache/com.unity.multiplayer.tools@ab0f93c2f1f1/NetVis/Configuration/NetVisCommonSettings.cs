using System;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetVis.Configuration
{
    [Serializable]
    class NetVisCommonSettings
    {
        public bool Outline { get; set; } = true;

        public const float k_SceneSaturationMin = 0f;
        public const float k_SceneSaturationMax = 1f;
        [Range(k_SceneSaturationMin, k_SceneSaturationMax)]
        public float SceneSaturation;
    }
}
