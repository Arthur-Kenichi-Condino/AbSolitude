using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetworkProfiler.Editor
{
    [Serializable]
    class DetailsViewSelectedState : ISerializationCallbackReceiver
    {
        [SerializeField]
        List<string> m_SelectedSerialized = new List<string>();

        /// <summary>
        /// Holds the uniquepath(path+id) of the items that are selected
        /// </summary>
        HashSet<string> m_Selected = new HashSet<string>();

        public void SetSelected(IReadOnlyCollection<string> locators, IReadOnlyCollection<ulong> ids)
        {
            m_Selected.Clear();

            var locatorList = locators.ToList();
            var idList = ids.ToList();

            if (locatorList.Count == 0)
            {
                return;
            }

            for (var i = 0; i < locators.Count; i++)
            {
                var uniquePath = locatorList[i] + idList[i];
                if (m_Selected.Contains(uniquePath))
                {
                    continue;
                }

                m_Selected.Add(uniquePath);
            }
        }

        public bool IsSelected(string locator)
        {
            return m_Selected.Contains(locator);
        }

        public void OnBeforeSerialize()
        {
            m_SelectedSerialized.Clear();
            m_SelectedSerialized.AddRange(m_Selected);
        }

        public void OnAfterDeserialize()
        {
            m_Selected.Clear();
            foreach (var value in m_SelectedSerialized)
            {
                m_Selected.Add(value);
            }
        }
    }
}
