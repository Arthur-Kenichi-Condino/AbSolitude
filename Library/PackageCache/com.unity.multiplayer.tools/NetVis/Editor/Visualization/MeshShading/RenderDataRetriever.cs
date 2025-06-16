using System.Collections.Generic;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetVis.Editor.Visualization
{
    /// <summary>
    /// Retrieves data from GameObject Renderers (like SkinnedMeshRenderer)
    /// </summary>
    class RenderDataRetriever
    {
        readonly List<Renderer> m_RenderersPin = new();

        public void WriteValueToRendererInstanceIdsInDictionary<TValue>(
            GameObject gameObject,
            TValue value,
            Dictionary<int, TValue> dictionary)
        {
            gameObject.GetComponentsInChildren(m_RenderersPin);
            foreach (var renderer in m_RenderersPin)
            {
                dictionary[renderer.GetInstanceID()] = value;
            }
        }
    }
}
