using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Multiplayer.Tools.NetworkSimulator.Runtime;
#if UNITY_6000_5_OR_NEWER
using UnityEngine.Assemblies;
#endif

namespace Unity.Multiplayer.Tools.NetworkSimulator.Editor.UI
{
    static class NetworkScenarioTypesLibrary
    {
        internal static IList<Type> Types { get; private set; }

        static NetworkScenarioTypesLibrary()
        {
            RefreshTypes();
        }

        internal static NetworkScenario GetInstanceForTypeName(string typeName)
        {
            var scenario = Types.First(x => x.Name == typeName);
            return (NetworkScenario)Activator.CreateInstance(scenario);
        }

        static void RefreshTypes()
        {
            if (Types != null)
            {
                return;
            }

#if UNITY_6000_5_OR_NEWER
            Types = CurrentAssemblies.GetLoadedAssemblies()
#else
            Types = AppDomain.CurrentDomain.GetAssemblies()
#endif
                .SelectMany(x => x.GetTypes())
                .Where(TypeIsValidNetworkScenario)
                .ToList();
        }

        static bool TypeIsValidNetworkScenario(Type type)
        {
            return type.IsClass && type.IsAbstract == false && typeof(NetworkScenario).IsAssignableFrom(type);
        }
    }
}
