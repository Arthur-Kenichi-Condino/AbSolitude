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
        
        static int StateObjectHashCode
        {
            get => SessionState.GetInt(nameof(DetailsViewPersistentState), -1);
            set => SessionState.SetInt(nameof(DetailsViewPersistentState), value);
        }
        
        static DetailsViewPersistentState GetOrCreateStateObject()
        {
            if (s_StateObject)
            {
                return s_StateObject;
            }
            
            if(StateObjectHashCode != -1)
            {
                var allStateObjects = Resources.FindObjectsOfTypeAll<DetailsViewPersistentState>();
                foreach(var stateObject in allStateObjects)
                {
                    if (stateObject.GetHashCode() == StateObjectHashCode)
                    {
                        return s_StateObject = stateObject;
                    }
                }
            }
            
            if (!s_StateObject)
            {
                s_StateObject = CreateInstance<DetailsViewPersistentState>();
                s_StateObject.hideFlags = HideFlags.HideAndDontSave;
                StateObjectHashCode = s_StateObject.GetHashCode();
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
