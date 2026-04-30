using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using UnityEngine.Assemblies;
#endif

namespace Unity.Multiplayer.Tools.NetStats
{
    internal static class TypeRegistration
    {
        public const string k_ClassName = "<NetStats_TypeRegistration>";
        public const string k_MethodName = "Run";

        static bool s_TypeRegistrationComplete;
        static readonly object s_LockObject = new object();

        public static void RunIfNeeded()
        {
            lock (s_LockObject)
            {
                if (s_TypeRegistrationComplete)
                {
                    return;
                }

                s_TypeRegistrationComplete = true;

#if UNITY_6000_5_OR_NEWER
                foreach (var assembly in CurrentAssemblies.GetLoadedAssemblies())
#else
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
#endif
                {
                    if (!assembly.GetCustomAttributes<AssemblyRequiresTypeRegistrationAttribute>().Any())
                    {
                        continue;
                    }

                    var runMethod = assembly
                        .GetType(k_ClassName)
                        ?.GetMethod(k_MethodName, BindingFlags.NonPublic | BindingFlags.Static);
                    if (runMethod == null)
                    {
                        Debug.LogError($"Failed to load type initialization for assembly {assembly.GetName().Name}");
                        continue;
                    }

                    runMethod.Invoke(null, null);
                }
                MetricIdTypeLibrary.TypeRegistrationPostProcess();
            }
        }
    }
}
