using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.Multiplayer.Tools.Editor.MultiplayerToolsWindow
{
    static class SceneToolInstance
    {
        /// Select the scene's existing <typeparamref name="T"/> if it has one, and otherwise create a
        /// new GameObject holding it.
        internal static void SelectExistingOrCreate<T>(string gameObjectName, string existingInstanceMessage)
            where T : Component
        {
            var existing = Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);
            if (existing != null)
            {
                var existingGameObject = existing.gameObject;
                Selection.activeGameObject = existingGameObject;
                EditorGUIUtility.PingObject(existingGameObject);
                Debug.Log(existingInstanceMessage, existingGameObject);
                return;
            }

            var go = new GameObject(gameObjectName);
            go.AddComponent<T>();
            Undo.RegisterCreatedObjectUndo(go, $"Add {gameObjectName} to scene");
            EditorGUIUtility.PingObject(go);
        }
    }
}
