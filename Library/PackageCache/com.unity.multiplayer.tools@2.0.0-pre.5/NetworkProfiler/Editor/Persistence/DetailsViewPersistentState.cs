using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetworkProfiler.Editor
{
    class DetailsViewPersistentState : ScriptableObject
    {
        static DetailsViewPersistentState s_StateObject;

        [SerializeField]
        DetailsViewFoldoutState m_FoldoutState = new DetailsViewFoldoutState();

        [SerializeField]
        DetailsViewSelectedState m_SelectedState = new DetailsViewSelectedState();

        [SerializeField]
        string m_SearchBarString;

        public static string SearchBarString
        {
            get => GetOrCreateStateObject().m_SearchBarString;
            set => GetOrCreateStateObject().m_SearchBarString = value;
        }
#if !UNITY_2022_1_OR_NEWER
        public class MostRecentlySelectedItem
        {
            public string path;
            public ulong id;

            public MostRecentlySelectedItem(string path, ulong id)
            {
                this.path = path;
                this.id = id;
            }
        }

        public static MostRecentlySelectedItem s_mostRecentlySelectedItem;
#endif

        static int? StateObjectInstanceId
        {
            get
            {
                var stateObjectInstanceId = SessionState.GetInt(nameof(DetailsViewPersistentState), -1);
                if (stateObjectInstanceId == -1)
                {
                    return null;
                }

                return stateObjectInstanceId;
            }
            set
            {
                if (!value.HasValue)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                SessionState.SetInt(nameof(DetailsViewPersistentState), value.Value);
            }
        }

        static DetailsViewPersistentState GetOrCreateStateObject()
        {
            if (s_StateObject)
            {
                return s_StateObject;
            }

            var maybeInstanceId = StateObjectInstanceId;
            if (maybeInstanceId.HasValue)
            {
                s_StateObject = EditorUtility.InstanceIDToObject(maybeInstanceId.Value) as DetailsViewPersistentState;
            }

            if (!s_StateObject)
            {
                s_StateObject = CreateInstance<DetailsViewPersistentState>();
                s_StateObject.hideFlags = HideFlags.HideAndDontSave;
                StateObjectInstanceId = s_StateObject.GetInstanceID();
            }

            return s_StateObject;
        }

        public static bool IsFoldedOut(string locator)
            => GetOrCreateStateObject().m_FoldoutState.IsFoldedOut(locator);

        public static void SetFoldout(string locator, bool isExpanded)
            => GetOrCreateStateObject().m_FoldoutState.SetFoldout(locator, isExpanded);

        public static void SetFoldoutExpandAll()
            => GetOrCreateStateObject().m_FoldoutState.SetFoldoutExpandAll();

        public static void SetFoldoutContractAll()
            => GetOrCreateStateObject().m_FoldoutState.SetFoldoutContractAll();

        public static bool IsSelected(string locator)
            => GetOrCreateStateObject().m_SelectedState.IsSelected(locator);

        public static void SetSelected(IReadOnlyList<string> pathList, IReadOnlyList<ulong> idList)
        {
            GetOrCreateStateObject().m_SelectedState.SetSelected(pathList, idList);
#if !UNITY_2022_1_OR_NEWER
            if (pathList.Count > 0)
            {
                s_mostRecentlySelectedItem = new MostRecentlySelectedItem(pathList[pathList.Count - 1], idList[idList.Count - 1]);
            }
#endif
        }
    }
}
