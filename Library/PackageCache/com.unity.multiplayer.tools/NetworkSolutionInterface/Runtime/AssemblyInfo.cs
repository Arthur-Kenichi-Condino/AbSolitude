using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Unity.Netcode.Runtime")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetworkProfiler.Editor")]

// Test assemblies
#if UNITY_INCLUDE_TESTS
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetworkSolutionInterface.Tests.Editor")]
#endif