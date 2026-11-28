using System;

namespace Unity.Multiplayer.Tools.NetStatsMonitor.Implementation
{
    struct GraphParameters
    {
        public int StatCount { get; set; }
        public int SamplesPerStat { get; set; }
    }

    struct GraphBufferParameters
    {
        public const float k_MaxPointsPerPixel = 1.0f;

        public int StatCount { get; set; }
        public int GraphWidthPoints { get; set; }

        internal GraphBufferParameters(in GraphParameters graphParams, float graphContentWidth, float maxPointsPerPixel)
        {
            StatCount = graphParams.StatCount;
            GraphWidthPoints = Math.Min((int)(maxPointsPerPixel * graphContentWidth), graphParams.SamplesPerStat);
        }
    }
}
