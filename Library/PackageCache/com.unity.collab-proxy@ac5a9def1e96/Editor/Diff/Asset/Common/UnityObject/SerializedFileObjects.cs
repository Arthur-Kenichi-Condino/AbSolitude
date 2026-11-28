using System;

using UnityEditorInternal;
using UnityEngine;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Common.UnityObject
{
    internal class SerializedFileObjects : IDisposable
    {
        internal UnityEngine.Object[] Objects { get { return mObjects; } }

        SerializedFileObjects(UnityEngine.Object[] objects)
        {
            mObjects = objects;
        }

        internal static SerializedFileObjects Read(string file)
        {
            UnityEngine.Object[] objects =
                InternalEditorUtility.LoadSerializedFileAndForget(file);

            if (objects == null)
                return new SerializedFileObjects(Array.Empty<UnityEngine.Object>());

            DetachFromScenes(objects);

            return new SerializedFileObjects(objects);
        }

        void IDisposable.Dispose()
        {
            if (mObjects == null)
                return;

            UnityEngine.Object[] objects = mObjects;
            mObjects = null;

            DestroyGameObjects(objects);
            DestroyRemainingObjects(objects);
        }

        // Unity registers every root Transform this produces as a root of the active scene,
        // so a later save would write them into its SceneRoots, mark HideAndDontSave to avoid it
        static void DetachFromScenes(UnityEngine.Object[] objects)
        {
            foreach (UnityEngine.Object obj in objects)
            {
                if (obj == null)
                    continue;

                obj.hideFlags |= HideFlags.HideAndDontSave;

                GameObject go = obj as GameObject;
                if (go == null)
                    continue;

                Transform transform = go.transform;
                if (transform != null)
                    transform.hideFlags |= HideFlags.HideAndDontSave;
            }
        }

        static void DestroyGameObjects(UnityEngine.Object[] objects)
        {
            foreach (UnityEngine.Object obj in objects)
            {
                if (obj == null)
                    continue;

                if (!(obj is GameObject))
                    continue;

                UnityEngine.Object.DestroyImmediate(obj, allowDestroyingAssets: true);
            }
        }

        // Components of the GameObjects destroyed above are already gone (Unity's overloaded ==
        // reports them null), so what reaches here is standalone objects plus any orphan Component
        // whose GameObject was not in the file. Unity refuses to destroy a Transform on its own.
        static void DestroyRemainingObjects(UnityEngine.Object[] objects)
        {
            foreach (UnityEngine.Object obj in objects)
            {
                if (obj == null)
                    continue;

                if (obj is Transform)
                    continue;

                UnityEngine.Object.DestroyImmediate(obj, allowDestroyingAssets: true);
            }
        }

        UnityEngine.Object[] mObjects;
    }
}
