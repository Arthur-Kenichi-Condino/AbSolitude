using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.MetricEvents")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetStatsMonitor.Component")]

// Test assemblies
#if UNITY_INCLUDE_TESTS
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetStatsMonitor.Implementation.Tests.Editor")]
#endif