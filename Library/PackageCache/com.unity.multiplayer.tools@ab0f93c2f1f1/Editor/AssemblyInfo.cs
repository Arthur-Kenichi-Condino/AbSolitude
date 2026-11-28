using System.Runtime.CompilerServices;

// Test assemblies
#if UNITY_INCLUDE_TESTS
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.BuildSettings.Tests.Editor")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.DependencyInjection.UIElements")]
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.NetVis.Editor.UI")]
#endif
