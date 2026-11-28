#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Multiplayer.Tools.Common
{
    // SyncedSingleton helps to synchronize data between Multiplayer Play mode instances through a file on the disk.
    // One Editor serializes the data to a file and the other Editors are notified about the file change
    // and will reload the data from that file.
    internal class SyncedSingleton<T> : ScriptableSingleton<T> where T : ScriptableObject
    {
        private static bool s_NeedsRegeneration;
        public static event Action DataChanged;

        // If the file is updated and the editor is not in focus and entered play mode
        // before focus is regained (e.g. multiplayer play mode), then the regeneration
        // method --called through delayCall-- could be executed after play mode.
        // To avoid this, we reload it on entering play mode state change. It's redundant
        // but safer.
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            ReloadIfNeeded();
        }

        static SyncedSingleton()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            s_NeedsRegeneration = true;
            ((IDisposable)source).Dispose();
            EditorApplication.delayCall += ReloadIfNeeded;
        }

        private static void ReloadIfNeeded()
        {
            if (s_NeedsRegeneration)
            {
                // By deleting the instance the Editor will reload the data from the file.
                DestroyImmediate(instance);
                s_NeedsRegeneration = false;
                DataChanged?.Invoke();
            }
        }

        private FileSystemWatcher m_FileWatcher;

        protected void OnEnable()
        {
            Assert.IsNull(m_FileWatcher);

            var directory = Path.GetDirectoryName(ScriptableSingleton<T>.GetFilePath());

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            m_FileWatcher = new FileSystemWatcher();
            m_FileWatcher.Path = directory;
            m_FileWatcher.Changed += OnFileChanged;
            m_FileWatcher.Created += OnFileChanged;
            m_FileWatcher.EnableRaisingEvents = true;
        }
    }
}
#endif