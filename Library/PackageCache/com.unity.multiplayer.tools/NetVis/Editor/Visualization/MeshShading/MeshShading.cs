using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.Multiplayer.Tools.Adapters;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.NetVis.Configuration;
using UnityEditor;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetVis.Editor.Visualization
{
    class MeshShading : IDisposable
    {
        [MaybeNull]
        static Camera SceneViewCamera => SceneView.sceneViews.Count > 0
            ? ((SceneView)SceneView.sceneViews[0]).camera
            : null;

        // External Dependencies:
        NetVisDataStore NetVisDataStore { get; }
        NetVisConfiguration m_Configuration;
        NetVisSettings Settings => m_Configuration.Settings;

        // Internal States:
        readonly ObjectColoring m_ObjectColoring;
        readonly RenderDataRetriever m_RenderDataRetriever = new();
        readonly Dictionary<int, Color> m_InstanceIdToColor = new();

        [MaybeNull] // Null in the event that no scene view camera is available
        NetVisRenderer m_NetVisRenderer;

        internal MeshShading(
            NetVisConfiguration configuration,
            NetVisDataStore netVisDataStore)
        {
            DebugUtil.TraceMethodName();

            NetVisDataStore = netVisDataStore;
            m_ObjectColoring = new ObjectColoring(netVisDataStore);

            OnConfigurationChanged(configuration);
        }

        public void Dispose()
        {
            DebugUtil.TraceMethodName();
            m_NetVisRenderer?.Dispose();
            m_NetVisRenderer = null;
        }

        public void OnConfigurationChanged(NetVisConfiguration configuration)
        {
            DebugUtil.TraceMethodName();
            m_Configuration = configuration;
            UpdateObjectColors();
        }

        public void UpdateObjectColors()
        {
            if (!NetVisDataStore.IsConnectedServerOrClient)
            {
                ClearColors();
                return;
            }
            
            var currentSceneViewCamera = SceneViewCamera;
            if (m_NetVisRenderer?.Camera != SceneViewCamera)
            {
                m_NetVisRenderer?.Dispose();
                m_NetVisRenderer = currentSceneViewCamera == null
                    ? null
                    : new(SceneViewCamera);
            }
            if (m_NetVisRenderer == null)
            {
                return;
            }

            m_NetVisRenderer.Outline = Settings.Common.Outline;
            m_NetVisRenderer.SceneSaturation = Settings.Common.SceneSaturation;

            ComputeNetworkedObjectColors(NetVisDataStore.GetObjectIds());

            m_NetVisRenderer.SetColorMapping(m_InstanceIdToColor);
        }
        
        void ClearColors()
        {
            m_InstanceIdToColor.Clear();
            m_ObjectColoring.UpdateForNewFrame(m_Configuration);
        }

        void ComputeNetworkedObjectColors(IReadOnlyList<ObjectId> objectIds)
        {
            ClearColors();
            
            if (m_Configuration.Metric == NetVisMetric.Bandwidth && NetVisDataStore.IsBandwidthCacheEmpty)
            {
                return;
            }

            foreach (var objectId in objectIds)
            {
                var gameObject = NetVisDataStore.GetGameObject(objectId);
                if (gameObject == null || !gameObject.activeSelf)
                {
                    continue;
                }

                if (!m_ObjectColoring.GetColor(objectId, out var color))
                {
                    continue;
                }

                m_RenderDataRetriever.WriteValueToRendererInstanceIdsInDictionary(gameObject, color, m_InstanceIdToColor);
            }
        }
    }
}
