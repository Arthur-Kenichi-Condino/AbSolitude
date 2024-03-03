using System;

namespace Unity.Multiplayer.Tools.Editor
{
    /// <summary>
    /// Methods to control whether the Runtime Net Stats Monitor is included in the build.
    /// When making automated builds of your project, you can use this to dynamically
    /// control whether the monitor is included in release or development builds.
    /// </summary>
    [Obsolete("This class is deprecated as the benefits of allowing the RNSM implementation to be conditionally compiled out of a build are too small. It will be removed in future.")]
    public static class NetStatsMonitorBuildSettings
    {
        // NOTE: These four public, parameterless methods are needed because our CI can only call
        // methods that are public and parameterless (not even optional parameters are allowed).

        /// <summary>
        /// Enables the RNSM in development builds for all build targets
        /// </summary>
        [Obsolete("This method is deprecated as the benefits of allowing the RNSM implementation to be conditionally compiled out of a build are too small. It will be removed in future.")]
        public static void EnableInDevelopForAllBuildTargets()
        {
            BuildSettings.SetSymbolForAllBuildTargets(Tool.RuntimeNetStatsMonitor, BuildSymbol.DisableInDevelop, false);
        }

        /// <summary>
        /// Disables the RNSM in development builds for all build targets
        /// </summary>
        [Obsolete("This method is deprecated as the benefits of allowing the RNSM implementation to be conditionally compiled out of a build are too small. It will be removed in future.")]
        public static void DisableInDevelopForAllBuildTargets()
        {
            BuildSettings.SetSymbolForAllBuildTargets(Tool.RuntimeNetStatsMonitor, BuildSymbol.DisableInDevelop, true);
        }

        /// <summary>
        /// Enables the RNSM in release builds for all build targets
        /// </summary>
        [Obsolete("This method is deprecated as the benefits of allowing the RNSM implementation to be conditionally compiled out of a build are too small. It will be removed in future.")]
        public static void EnableInReleaseForAllBuildTargets()
        {
            BuildSettings.SetSymbolForAllBuildTargets(Tool.RuntimeNetStatsMonitor, BuildSymbol.EnableInRelease, true);
        }

        /// <summary>
        /// Disables the RNSM in release builds for all build targets
        /// </summary>
        [Obsolete("This method is deprecated as the benefits of allowing the RNSM implementation to be conditionally compiled out of a build are too small. It will be removed in future.")]
        public static void DisableInReleaseForAllBuildTargets()
        {
            BuildSettings.SetSymbolForAllBuildTargets(Tool.RuntimeNetStatsMonitor, BuildSymbol.EnableInRelease, false);
        }
    }
}
