using System;
using Unity.Profiling.Editor;

namespace Unity.Multiplayer.Tools.NetworkProfiler.Editor
{
    [Serializable]
    [ProfilerModuleMetadata(ModuleNames.Message)]
    class NetcodeMessagesProfilerModule : ProfilerModule
    {
        public NetcodeMessagesProfilerModule()
            : base(ProfilerModuleDefinitions.MessagesProfilerModule.CountersAsDescriptors())
        {
        }

        public override ProfilerModuleViewController CreateDetailsViewController()
        {
            return new NetworkDetailsViewController(ProfilerWindow, TabNames.Messages);
        }
    }
}
