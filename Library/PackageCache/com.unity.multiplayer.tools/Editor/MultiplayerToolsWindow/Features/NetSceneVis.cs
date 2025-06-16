using System;
using UnityEditor;
using UnityEngine;
#if NETVIS_AVAILABLE
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
        
#if NETVIS_AVAILABLE && UNITY_NETCODE_GAMEOBJECTS_1_1_ABOVE
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
        
#if UNITY_2023_1_OR_NEWER
        public string AvailabilityMessage => "Network Scene Visualization is only available from version 2023.1.14f1, with Netcode for GameObjects 1.1+";
#elif UNITY_2022_1_OR_NEWER
        public string AvailabilityMessage => "Network Scene Visualization is only available from version 2022.3.11f1, with Netcode for GameObjects 1.1+";
#else
        public string AvailabilityMessage => "Network Scene Visualization is only available from version 2022.3.11f1+ for Unity 2022 or 2023.1.14f1+ for Unity 2023, with Netcode for GameObjects 1.1+";
#endif
        public void Open() => throw new NotImplementedException();
#endif
    }
}
