using UnityEngine.Profiling;

namespace Unity.Multiplayer.Tools.Common
{
    /// <summary>
    /// A Helper class for Profiler scoping
    /// </summary>
    public ref struct ProfilerScope
    {
        /// <summary>
        /// Static method returning a new ProfilerScope object and starting the sampling
        /// </summary>
        /// <param name="name">The name of the Profiler Sample</param>
        /// <returns>The ProfilerScope object</returns>
        public static ProfilerScope BeginSample(string name)
        {
            return new ProfilerScope(name);
        }

        ProfilerScope(string name)
        {
            Profiler.BeginSample(name);
        }

        /// <summary>
        /// Stopping the Profiler sampling on Disposal of the object (end of scope)
        /// </summary>
        public void Dispose()
        {
            Profiler.EndSample();
        }
    }
}
