using System;
using UnityEditor;
using UnityEngine;
#if UNITY_2023_2_OR_NEWER
using UnityEditor.Overlays;
#endif

namespace Unity.Multiplayer.Tools.Editor.MultiplayerToolsWindow
{
    class NetSceneVis : IMultiplayerToolsFeature
    {
        public string Name => "Network Scene Visualization";
        public string ToolTip => "Overlay info in the scene view (ownership, bandwidth)";
        public string ButtonText => "Open";
        public string DocumentationUrl => "https://docs-multiplayer.unity3d.com/tools/current/netscenevis/";
        
#if UNITY_2023_2_OR_NEWER && UNITY_NETCODE_GAMEOBJECTS_1_1_ABOVE
        public bool IsAvailable => true;
        public string AvailabilityMessage => "Available";

        public void Open()
        {
            var sceneView = EditorWindow.GetWindow<SceneView>();
            var overlayFound = sceneView.TryGetOverlay("Network Visualization", out Overlay match);
            if (overlayFound)
            {
                match.displayed = true;
                match.Undock();
            }
            else
            {
                Debug.LogWarning("Network Scene Visualization overlay not found");
            }       
        }
#else
        public bool IsAvailable => false;
        public string AvailabilityMessage => "Network Scene Visualization is only available in Unity 2023.2+, with Netcode for GameObjects 1.1+";
        public void Open() => throw new NotImplementedException();
#endif
    }
}
