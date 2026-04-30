using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetworkSimulator.Editor")]

#if UNITY_INCLUDE_TESTS
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetworkSimulator.Tests.Runtime")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
#endif
