using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.MetricEvents")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetworkProfiler.Editor")]

// Test assemblies
#if UNITY_INCLUDE_TESTS
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetworkProfiler.Tests.Editor")]
#endif
