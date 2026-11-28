using System;
using UnityEditor.Build;
using UnityEngine;

#if UNITY_GAMECORE
using UnityEditor.GameCore;
#endif

namespace UnityEditor.TestTools.TestRunner
{
    internal class GameCorePlatformSetup : IPlatformSetup
    {
#if UNITY_GAMECORE
        private const GameCoreDeployMethod k_deployMethod = GameCoreDeployMethod.Push;
        private const string k_PlsSize = "512";
        private const string k_PlsGrow = "1024";
        private const string k_TitleID = "73ECA03C";
        private const string k_MSAAppId = "0000000040283475";
        private const string k_SCID = "00000000-0000-0000-0000-000073ECA03C";


        private string m_Warning;

        private static GameCoreSettings GetGameCoreSettings()
        {
#if UNITY_GAMECORE_XBOXSERIES
            return GameCoreScarlettSettings.GetInstance();
#elif UNITY_GAMECORE_XBOXONE
            return GameCoreXboxOneSettings.GetInstance();
#endif
        }
        bool IsValidMsaAppId(string id)
        {
            if (id == null || id.Length != 16 || id == "0000000000000000")
                return false;

            for (int i = 0; i < id.Length; i++)
            {
                if (!Uri.IsHexDigit(id[i]))
                    return false;
            }

            return true;
        }

        bool IsValidScidAndTitleId(string scid, string titleId)
        {
            if (titleId == null || titleId.Length != 8 || titleId == "00000000")
                return false;

            for (int i = 0; i < titleId.Length; i++)
            {
                if (!Uri.IsHexDigit(titleId[i]))
                    return false;
            }

            if (!Guid.TryParse(scid, out _) || scid == "00000000-0000-0000-0000-000000000000")
                return false;

            if (!string.IsNullOrEmpty(titleId) && !scid.EndsWith(titleId, StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        bool IsValidPersistentLocalStorage(GameCore.GameConfig.CT_PersistentLocalStorage pls)
        {
            return pls != null && !string.IsNullOrEmpty(pls.SizeMB) && !string.IsNullOrEmpty(pls.GrowableToMB);
        }

#endif

        public void Setup()
        {
#if UNITY_GAMECORE
            GameCoreSettings settings = GetGameCoreSettings();

            if (settings == null)
                return;

            settings.DeploymentMethod = k_deployMethod;
            bool changed = false;

            if (!IsValidMsaAppId(settings.GameConfig.MSAAppId))
            {
                settings.GameConfig.MSAAppId = k_MSAAppId;
                changed = true;
            }

            if (!IsValidScidAndTitleId(settings.SCID, settings.GameConfig.TitleId))
            {
                m_Warning = $"Invalid SCID '{settings.SCID}' or TitleId '{settings.GameConfig.TitleId}'. Setting to default values (TitleId: {k_TitleID}, SCID: {k_SCID}).";
                settings.GameConfig.TitleId = k_TitleID;
                settings.SCID = k_SCID;
                changed = true;
            }

            if (!IsValidPersistentLocalStorage(settings.GameConfig.PersistentLocalStorage))
            {
                if (settings.GameConfig.PersistentLocalStorage == null)
                {
                    settings.GameConfig.PersistentLocalStorage = new GameCore.GameConfig.CT_PersistentLocalStorage();
                }
                settings.GameConfig.PersistentLocalStorage.SizeMB = k_PlsSize;
                settings.GameConfig.PersistentLocalStorage.GrowableToMB = k_PlsGrow;
                changed = true;
            }

            if (changed)
            {
                settings.SaveGameConfig();
            }
#endif
        }

        public void PostBuildAction()
        {
        }

        public void PostSuccessfulBuildAction()
        {
        }

        public void PostSuccessfulLaunchAction()
        {
        }

        public void CleanUp()
        {
#if UNITY_GAMECORE
            if (!string.IsNullOrEmpty(m_Warning))
            {
                Debug.LogWarning(m_Warning);
                m_Warning = null;
            }
#endif
        }
    }
}
