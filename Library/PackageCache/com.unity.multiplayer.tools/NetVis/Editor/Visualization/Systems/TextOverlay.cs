using System;
using System.Diagnostics.CodeAnalysis;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.NetVis.Configuration;
using UnityEditor;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetVis.Editor.Visualization
{
    class TextOverlay : IDisposable
    {
        [NotNull]
        NetVisConfiguration m_Configuration;

        [NotNull]
        readonly NetVisDataStore m_NetVisDataStore;

        static GUIStyle s_TextLabelStyle;

        public TextOverlay(
            [NotNull] NetVisConfiguration configuration,
            [NotNull] NetVisDataStore netVisDataStore)
        {
            DebugUtil.TraceMethodName();
            m_Configuration = configuration;
            m_NetVisDataStore = netVisDataStore;
        }

        public void Dispose()
        {
            DebugUtil.TraceMethodName();
        }

        public void OnConfigurationChanged(NetVisConfiguration configuration)
        {
            m_Configuration = configuration;
        }

        public void DuringSceneGui(SceneView sceneView)
        {
            if (!m_NetVisDataStore.IsConnectedServerOrClient)
                return;
            
            InitializeTextLabelStyle();

            var sceneViewCamera = sceneView.camera;
            var sceneViewCameraPosition = sceneViewCamera.transform.position;
            var screenRect = new Rect(0, 0, Screen.width, Screen.height);

            var bandwidthAvailable = !m_NetVisDataStore.IsBandwidthCacheEmpty;
            
            foreach (var id in m_NetVisDataStore.GetObjectIds())
            {
                var gameObject = m_NetVisDataStore.GetGameObject(id);
                if (gameObject == null)
                    continue;

                // Skip if object has no Colliders as we cannot raycast.
                if (!ObjectHasCollider(gameObject, out var collider, out var collider2D, out var colliderPosition))
                    continue;
                
                // Skip if object is not visible in scene view.
                if (!screenRect.Contains(sceneViewCamera.WorldToScreenPoint(colliderPosition)))
                    continue;
                
                // Raycast colliders to check if object is visible or obstructed.
                if (collider)
                {
                    if (!ObjectIsVisibleToCamera(sceneViewCameraPosition, gameObject, colliderPosition))
                        continue;
                }
                else if (collider2D)
                {
                    if (!ObjectIsVisibleToCamera(collider2D, colliderPosition))
                        continue;
                }
                
                var content = m_Configuration.Metric switch
                {
                    NetVisMetric.Bandwidth => bandwidthAvailable? m_NetVisDataStore.GetBandwidth(id).ToString("N0"):"No data",
                    NetVisMetric.Ownership => m_NetVisDataStore.GetOwner(id).ToString(),
                    _ => string.Empty,
                };

                Handles.Label(colliderPosition, content, s_TextLabelStyle);
            }
        }

        // We can't assign this in the constructor, or Unity will warn us that we can
        // only use GUI functions inside OnGui
        static void InitializeTextLabelStyle()
        {
            s_TextLabelStyle ??= new GUIStyle
            {
                padding = new RectOffset(2, 0, 0, 0),
                normal =
                {
                    textColor = Color.black,
                }
            };

            // For some reason the background texture is being reset to null in BossRoom when the scene view is
            // initially open. We're unsure as to why this happens, as it is not occurring in projects other than
            // BossRoom, or when the scene view is not initially open.
            // To work around this we can continue to re-create the background texture field whenever it is null.
            if (s_TextLabelStyle.normal.background == null)
            {
                // Unexpectedly, this produces a white background
                var backgroundTexture = new Texture2D(1, 1);
                backgroundTexture.SetPixels(new[] { Color.black });
                s_TextLabelStyle.normal.background = backgroundTexture;
            }
        }

        static bool ObjectIsVisibleToCamera(Vector3 cameraPosition, GameObject targetObject, Vector3 targetPosition)
        {
            var raycastHit = Physics.Raycast(cameraPosition, targetPosition - cameraPosition, out var hit);
            return raycastHit && hit.transform == targetObject.transform;
        }

        static bool ObjectIsVisibleToCamera(Collider2D targetCollider2D, Vector3 targetPosition)
        {
            var collider2D = Physics2D.OverlapPoint(targetPosition);
            return collider2D == targetCollider2D;
        }
        
        static bool ObjectHasCollider(GameObject gameObject, out Collider collider, out Collider2D collider2D, out Vector3 colliderPosition)
        {
            collider = null;
            collider2D = null;
            colliderPosition = new Vector3();
            
            if (gameObject.TryGetComponent(out collider))
            {
                colliderPosition = collider.bounds.center;
                return true;
            }

            if (gameObject.TryGetComponent(out collider2D))
            {
                colliderPosition = collider2D.bounds.center;
                return true;
            }

            return false;
        }
    }
}
