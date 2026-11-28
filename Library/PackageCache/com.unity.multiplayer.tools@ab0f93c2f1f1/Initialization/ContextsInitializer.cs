// Uncomment the line below to remove initialization and tear-down logs.
// #define UNITY_MP_TOOLS_CONTEXT_TRACE_CALLS

// Uncomment the line bellow to simulate a Build environment, so then you can check whether
// Runtime Contexts are correctly running or if Editor Only Contexts are disabled as expected.
// #define UNITY_MP_TOOLS_SIMULATE_BUILD

using Unity.Multiplayer.Tools.Common;
using UnityEngine;
using UnityEngine.Scripting;

[assembly: AlwaysLinkAssembly]
namespace Unity.Multiplayer.Tools.Context
{
    /// <summary>
    /// Main entry point initializing, enabling and disabling all Tools-specific Contexts.
    /// This class is automatically initialized in Editor and at Runtime.
    /// No other class or assembly should reference this class.
    /// </summary>
#if UNITY_EDITOR && !UNITY_MP_TOOLS_SIMULATE_BUILD
    [UnityEditor.InitializeOnLoad]
#endif
    static class ContextsInitializer
    {
        static IContext[] s_Contexts;
        static bool s_RuntimeContextsEnabled;

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#endif
        static void ReinitializeEditorContexts()
        {
            DebugUtil.TraceMethodName();
            if (s_Contexts != null && s_RuntimeContextsEnabled)
            {
                Application.quitting -= DisableRuntimeContexts;
                DisableRuntimeContexts();
            }
            
            s_Contexts = ContextsDefinition.GetContextsDefinitions();
#if UNITY_EDITOR && !UNITY_MP_TOOLS_SIMULATE_BUILD
            EnableEditorContexts();
#endif
        }

        static ContextsInitializer()
        {
            ReinitializeEditorContexts();
        }


        static void EnableEditorContexts()
        {
            DebugUtil.TraceMethodName();

            foreach (var context in s_Contexts)
            {
                if (context is IEditorSetupHandler editorContext)
                {
                    editorContext.EditorSetup();
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void EnableRuntimeContexts()
        {
            DebugUtil.TraceMethodName();

            Application.quitting += DisableRuntimeContexts;
            s_RuntimeContextsEnabled = true;
            foreach (var context in s_Contexts)
            {
                if (context is IRuntimeSetupHandler runtimeContext)
                {
                    runtimeContext.RuntimeSetup();
                }
            }
        }

        static void DisableRuntimeContexts()
        {
            DebugUtil.TraceMethodName();

            s_RuntimeContextsEnabled = false;
            foreach (var context in s_Contexts)
            {
                if (context is IRuntimeSetupHandler runtimeContext)
                {
                    runtimeContext.RuntimeTeardown();
                }
            }
        }
    }
}
