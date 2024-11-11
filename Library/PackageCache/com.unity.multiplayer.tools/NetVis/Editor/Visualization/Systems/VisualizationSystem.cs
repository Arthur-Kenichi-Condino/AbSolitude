using System;
using System.Diagnostics.CodeAnalysis;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.NetVis.Configuration;
using UnityEditor;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetVis.Editor.Visualization
{
    class VisualizationSystem : IDisposable
    {
        // External dependencies
        //---------------------------------------------------------------------
        readonly NetVisConfigurationWithEvents m_ConfigurationWithEvents;
        NetVisConfiguration Configuration => m_ConfigurationWithEvents.Configuration;
        readonly NetVisDataStore m_DataStore;

        // Members
        //---------------------------------------------------------------------
        [MaybeNull]
        MeshShading m_MeshShading;

        [MaybeNull]
        TextOverlay m_TextOverlay;

        internal VisualizationSystem(
            NetVisConfigurationWithEvents configurationWithEvents,
            NetVisDataStore netVisDataStore,
            IRuntimeUpdater runtimeUpdater)
        {
            DebugUtil.TraceMethodName();

            m_ConfigurationWithEvents = configurationWithEvents;
            m_DataStore = netVisDataStore;

            m_ConfigurationWithEvents.ConfigurationChanged += OnConfigurationChanged;

            EditorApplication.pauseStateChanged += OnPauseStateChanged;

            SceneView.duringSceneGui += DuringSceneGui;
            OnConfigurationChanged(configurationWithEvents.Configuration);
        }

        public void Dispose()
        {
            DebugUtil.TraceMethodName();
            SceneView.duringSceneGui -= DuringSceneGui;
            m_ConfigurationWithEvents.ConfigurationChanged -= OnConfigurationChanged;
            
            EditorApplication.pauseStateChanged -= OnPauseStateChanged;

            m_MeshShading?.Dispose();
            m_MeshShading = null;

            m_TextOverlay?.Dispose();
            m_TextOverlay = null;
        }

        void OnPauseStateChanged(PauseState pauseState)
        {
            DebugUtil.TraceMethodName();
            m_DataStore.OnPauseStateChanged(pauseState);
        }

        void DuringSceneGui(SceneView sceneView)
        {
            m_MeshShading?.UpdateObjectColors();
            m_TextOverlay?.DuringSceneGui(sceneView);
        }

        void OnConfigurationChanged(NetVisConfiguration configuration)
        {
            DebugUtil.TraceMethodName();

            m_DataStore.OnConfigurationChanged(configuration);

            Debug.Assert(Application.isPlaying);

            var metricSelected = configuration.Metric != NetVisMetric.None;
            var meshShadingRequired = metricSelected && configuration.MeshShadingEnabled;
            var textOverlayRequired = metricSelected && configuration.TextOverlayEnabled;

            if (meshShadingRequired)
            {
                if (m_MeshShading == null)
                {
                    m_MeshShading = new(Configuration, m_DataStore);
                }
                else
                {
                    m_MeshShading.OnConfigurationChanged(Configuration);
                }
            }
            else
            {
                m_MeshShading?.Dispose();
                m_MeshShading = null;
            }

            if (textOverlayRequired)
            {
                if (m_TextOverlay == null)
                {
                    m_TextOverlay = new(Configuration, m_DataStore);
                }
                else
                {
                    m_TextOverlay.OnConfigurationChanged(Configuration);
                }
            }
            else
            {
                m_TextOverlay?.Dispose();
                m_TextOverlay = null;
            }
        }
    }
}
