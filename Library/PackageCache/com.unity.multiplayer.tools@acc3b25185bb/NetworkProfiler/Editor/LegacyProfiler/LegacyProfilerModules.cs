#if ENABLE_PROFILER
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetworkProfiler.Editor
{
    [InitializeOnLoad]
    static class LegacyProfilerModules
    {
#pragma warning disable IDE1006 // disable naming rule violation check
        [Serializable]
        internal class NetcodeProfilerCounter
        {
            // Note: These fields are named this way for internal serialization aligned w/ Unity profiler metadata
            public string m_Name;
            public string m_Category;
        }

        [Serializable]
        internal class NetcodeProfilerModuleData
        {
            // Note: These fields are named this way for internal serialization aligned w/ Unity profiler metadata
            public List<NetcodeProfilerCounter> m_ChartCounters = new List<NetcodeProfilerCounter>();
            public List<NetcodeProfilerCounter> m_DetailCounters = new List<NetcodeProfilerCounter>();
            public string m_Name;
        }

        [Serializable]
        internal class NetcodeModules
        {
            // Note: These fields are named this way for internal serialization aligned w/ Unity profiler metadata
            public List<NetcodeProfilerModuleData> m_Modules;
        }
#pragma warning restore IDE1006 // restore naming rule violation check

        static readonly string[] k_LegacyModuleNames =
        {
            // Profiler V1 naming within MLAPI
            "MLAPI RPCs",
            "MLAPI Operations",
            "MLAPI Messages",
        };

        const string k_LegacyDynamicModulesName = "ProfilerWindow.DynamicModules";

        static void CleanAllLegacyModules()
        {
            var dynamicModules = GetDynamicModules();
            if (dynamicModules == null)
            {
                return;
            }

            var modules = dynamicModules.m_Modules;
            bool cleanedSome = false;

            cleanedSome |= 0 < modules.RemoveAll(module => k_LegacyModuleNames.Contains(module.m_Name));

            cleanedSome |= 0 < modules.RemoveAll(
                legacyModule => ProfilerModuleDefinitions.Modules.Any(
                    module => module.Name.Equals(legacyModule.m_Name)));

            if (cleanedSome)
            {
                SaveDynamicModules(dynamicModules);
            }
        }

        static LegacyProfilerModules()
        {
            // Clear all legacy modules

            // Although legacy modules were only ever added in Unity versions <= 2020.3, and although we dropped support
            // for Unity < 2021.3 in Tools 2.0, we should keep this code to remove legacy modules around to ensure
            // that any lingering legacy modules are still removed after an upgrade to Tools 2.0

            CleanAllLegacyModules();
        }

        static NetcodeModules GetDynamicModules()
        {
            var dynamicModulesJson = EditorPrefs.GetString(k_LegacyDynamicModulesName);
            return JsonUtility.FromJson<NetcodeModules>(dynamicModulesJson);
        }

        static void SaveDynamicModules(NetcodeModules dynamicModules)
        {
            EditorPrefs.SetString(k_LegacyDynamicModulesName, JsonUtility.ToJson(dynamicModules));
        }
    }
}
#endif
