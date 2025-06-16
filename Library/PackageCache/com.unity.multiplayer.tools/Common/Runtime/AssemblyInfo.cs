using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.Adapters")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.Adapters.MockNgo")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.Adapters.Ngo1")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.Common.Tests")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.Editor")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.Initialization")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.MetricTestData")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.MetricTypes")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetStats")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetStatsMonitor.Component")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetStatsMonitor.Configuration")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetStatsMonitor.Editor")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetStatsMonitor.Implementation")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetworkProfiler.Editor")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetworkSimulator.Editor")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetworkSimulator.Runtime")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetVis.Configuration")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetVis.Editor.UI")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetVis.Editor.Visualization")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.DependencyInjection")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.DependencyInjection.UIElements")]

// Test assemblies
#if UNITY_INCLUDE_TESTS
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.Common.Tests.Editor")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetVis.Tests.Editor")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetworkSimulator.Tests.Runtime")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetStatsMonitor.Implementation.Tests.Editor")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetworkProfiler.Tests.Editor")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
#endif
