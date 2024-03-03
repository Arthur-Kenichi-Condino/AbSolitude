using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.Initialization")]

// Test assemblies
#if UNITY_INCLUDE_TESTS
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetVis.Tests.Editor")]
#endif
