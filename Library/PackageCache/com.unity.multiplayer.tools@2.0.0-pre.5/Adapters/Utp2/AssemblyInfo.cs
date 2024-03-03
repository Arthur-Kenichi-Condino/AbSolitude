using System.Runtime.CompilerServices;

// Adapters
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.Adapters.Ngo1WithUtp2")]

// Test assemblies
#if UNITY_INCLUDE_TESTS
[assembly: InternalsVisibleTo("Unity.Multiplayer.Tools.Adapters.Tests.Editor")]
#endif
