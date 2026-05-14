using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetStatsMonitor.Component")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetStatsMonitor.Editor")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetStatsMonitor.Implementation")]

// Test assemblies
#if UNITY_INCLUDE_TESTS
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetStatsMonitor.Implementation.Tests.Editor")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetStatsMonitor.Interface.Tests.Editor")]
#endif