using System;
using System.IO;
using System.Reflection;

using UnityEngine;

using Codice.Utils;

namespace Unity.PlasticSCM.Editor
{
    internal class UnityAssemblyLocationProvider : AssemblyLocation.IAssemblyLocationProvider
    {
        private UnityAssemblyLocationProvider() { }

        internal static void Register()
        {
            // We keep a single instance to avoid extra instantiations from the toolbar.
            // It can be simplified once we configure a single package entry point.
            if (mUnityAssemblyLocationProvider == null)
                mUnityAssemblyLocationProvider = new UnityAssemblyLocationProvider();

            AssemblyLocation.SetAssemblyLocationProvider(mUnityAssemblyLocationProvider);
        }

        public string GetForAssembly(Assembly assembly)
        {
            string assemblyLocation = GetAssemblyLocation(assembly);

            if (!string.IsNullOrEmpty(assemblyLocation))
                return Path.GetDirectoryName(assemblyLocation);

#if ENABLE_CORECLR && UNITY_6000_5_OR_NEWER
            // Assembly.Location is empty for the stream-loaded assemblies CoreCLR hands us.
            // Without a Location, AssemblyLocation.GetAssemblyDirectory() returns the runtime directory
            // instead of Lib/Editor. The Editor still knows the path the assembly was loaded from since 6.5.
            string assemblyPath = assembly.GetLoadedAssemblyPath();

            if (!string.IsNullOrEmpty(assemblyPath))
                return Path.GetDirectoryName(assemblyPath);
#endif

            return Path.GetDirectoryName(GetDefaultAssemblyLocation(assembly));
        }

        static string GetDefaultAssemblyLocation(Assembly assembly)
        {
            string assemblyLocation = GetAssemblyLocation(assembly);
            string codebase = assembly.EscapedCodeBase;

            if (codebase == null)
                return assemblyLocation;

            Uri uri;
            if (!Uri.TryCreate(codebase, UriKind.Absolute, out uri))
                return assemblyLocation;

            if (uri.IsFile)
                return uri.LocalPath;

            return assemblyLocation;
        }

        static string GetAssemblyLocation(Assembly assembly)
        {
#pragma warning disable UAC0007
            return assembly.Location;
#pragma warning restore UAC0007
        }

        private static UnityAssemblyLocationProvider mUnityAssemblyLocationProvider;
    }
}
