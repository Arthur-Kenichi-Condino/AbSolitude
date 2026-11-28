using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.TestTools;

namespace Unity.Multiplayer.Tools.Tests
{
    /// <summary>
    /// Edit-mode smoke tests for the tools package across play-mode cycles, with and without fast
    /// enter play mode (toggled in-test), preceded by an edit-mode domain reload.
    /// NOTE: with fast enter play mode disabled, because of domain reload each test method will have its own entry/cycle.
    /// </summary>
    internal class MultiplayerToolsPlayModeCycleTests
    {
        const int k_FastPlayModeCycles = 3;
        const string k_SettingsSavedKey = "PlayModeCycleTests_SettingsSaved";
        const string k_SavedEnabledKey = "PlayModeCycleTests_SavedEnterPlayModeOptionsEnabled";
        const string k_SavedOptionsKey = "PlayModeCycleTests_SavedEnterPlayModeOptions";
        const string k_ReloadRequestedKey = "PlayModeCycleTests_ReloadRequested";
        const string k_TeardownReloadRequestedKey = "PlayModeCycleTests_TeardownReloadRequested";

        [UnityTest]
        public IEnumerator PlayModeCyclesAfterDomainReload_WithFastEnterPlayMode_RaiseNoErrors()
            => CycleThroughPlayMode(fastEnterPlayMode: true, cycles: k_FastPlayModeCycles, reloadDomainBeforeEntering: true);

        [UnityTest]
        public IEnumerator PlayModeCycleAfterDomainReload_WithoutFastEnterPlayMode_RaisesNoErrors()
            => CycleThroughPlayMode(fastEnterPlayMode: false, cycles: 1, reloadDomainBeforeEntering: true);

        [UnityTest]
        public IEnumerator PlayModeCycle_WithoutFastEnterPlayMode_RaisesNoErrors()
            => CycleThroughPlayMode(fastEnterPlayMode: false, cycles: 1, reloadDomainBeforeEntering: false);

        IEnumerator CycleThroughPlayMode(bool fastEnterPlayMode, int cycles, bool reloadDomainBeforeEntering)
        {
            // Persist state in SessionState (not fields): domain reloads survive it, fields don't.
            if (!SessionState.GetBool(k_SettingsSavedKey, false))
            {
                SessionState.SetBool(k_SettingsSavedKey, true);
                SessionState.SetInt(k_SavedEnabledKey, EditorSettings.enterPlayModeOptionsEnabled ? 1 : 0);
                SessionState.SetInt(k_SavedOptionsKey, (int)EditorSettings.enterPlayModeOptions);
            }
            EditorSettings.enterPlayModeOptionsEnabled = fastEnterPlayMode;
            // Fast enter play mode disables BOTH reloads, matching the Unity 6.6 default.
            EditorSettings.enterPlayModeOptions = fastEnterPlayMode
                ? EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload
                : EnterPlayModeOptions.None;

            if (reloadDomainBeforeEntering)
            {
                if (!SessionState.GetBool(k_ReloadRequestedKey, false))
                {
                    SessionState.SetBool(k_ReloadRequestedKey, true);
                    EditorUtility.RequestScriptReload();
                }
                yield return new WaitForDomainReload();
            }

            for (var cycle = 0; cycle < cycles; cycle++)
            {
                yield return new EnterPlayMode();
                Assert.IsTrue(EditorApplication.isPlaying, $"Expected the editor to be in play mode on cycle {cycle}.");
                yield return new ExitPlayMode();
            }
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            // If the test failed while a play-mode enter was still pending, wait for it to settle first, so the next test never starts mid-transition.
            // Frame-bounded so a cancelled transition won't hang the teardown.
            for (var i = 0; i < 1000 && EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying; i++)
            {
                yield return null;
            }
            if (EditorApplication.isPlaying)
            {
                yield return new ExitPlayMode();
            }

            SessionState.EraseBool(k_ReloadRequestedKey);
            if (SessionState.GetBool(k_SettingsSavedKey, false))
            {
                EditorSettings.enterPlayModeOptionsEnabled = SessionState.GetInt(k_SavedEnabledKey, 0) == 1;
                EditorSettings.enterPlayModeOptions = (EnterPlayModeOptions)SessionState.GetInt(k_SavedOptionsKey, 0);
                SessionState.EraseBool(k_SettingsSavedKey);
                SessionState.EraseInt(k_SavedEnabledKey);
                SessionState.EraseInt(k_SavedOptionsKey);
            }

            if (!SessionState.GetBool(k_TeardownReloadRequestedKey, false))
            {
                SessionState.SetBool(k_TeardownReloadRequestedKey, true);
                EditorUtility.RequestScriptReload();
                yield return new WaitForDomainReload();
            }
            SessionState.EraseBool(k_TeardownReloadRequestedKey);
        }
    }
}
