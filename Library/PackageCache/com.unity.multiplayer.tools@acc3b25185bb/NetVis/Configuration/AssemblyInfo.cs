using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetVis.Editor.UI")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetVis.Editor.Visualization")]

// Test assemblies
#if UNITY_INCLUDE_TESTS
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetVis.Tests.Configuration.Editor")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetVis.Tests.Editor")]
#endif
