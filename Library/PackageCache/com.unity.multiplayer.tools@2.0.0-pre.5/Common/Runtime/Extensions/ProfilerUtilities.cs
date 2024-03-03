using UnityEngine.Profiling;

namespace Unity.Multiplayer.Tools.Common
{
    public ref struct ProfilerScope
    {
        public static ProfilerScope BeginSample(string name)
        {
            return new ProfilerScope(name);
        }

        ProfilerScope(string name)
        {
            Profiler.BeginSample(name);
        }

        public void Dispose()
        {
            Profiler.EndSample();
        }
    }
}
