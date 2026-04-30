namespace Unity.Multiplayer.Tools.NetStatsMonitor.Implementation
{
    struct LinearTransform
    {
        /// As in y = a * x + b
        public float A { get; set; }

        /// As in y = a * x + b
        public float B { get; set; }

        public float Apply(float x)
        {
            // Math.FusedMultiplyAdd doesn't seem to be available for some reason.
            // Hopefully this optimization is still possible with Burst
            return A * x + B;
        }

        public bool IsIdentity => A == 1f && B == 0f;

        public static LinearTransform Identity => new LinearTransform { A = 1, B = 0 };
    }
}
