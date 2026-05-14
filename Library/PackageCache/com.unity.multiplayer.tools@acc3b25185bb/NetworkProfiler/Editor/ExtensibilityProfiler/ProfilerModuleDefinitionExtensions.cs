using System.Linq;
using Unity.Profiling;
using Unity.Profiling.Editor;

namespace Unity.Multiplayer.Tools.NetworkProfiler.Editor
{
    static class ProfilerModuleDefinitionExtensions
    {
        public static ProfilerCounterDescriptor[] CountersAsDescriptors(this ProfilerModuleDefinition moduleDefinition)
        {
            return moduleDefinition.Counters
                .Select(counter => new ProfilerCounterDescriptor(counter, ProfilerCategory.Network.Name))
                .ToArray();
        }
    }
}
