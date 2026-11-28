using System;
using Unity.Multiplayer.Tools.Common.Helpers;
using Unity.Multiplayer.Tools.Editor.MultiplayerToolsWindow.Analytics;
using UnityEditor;
using UnityEngine;
using UnityEditor.Overlays;

namespace Unity.Multiplayer.Tools.Editor.MultiplayerToolsWindow
{
    class NetSceneVis : IMultiplayerToolsFeature
    {
        public string Name => "Network Scene Visualization";
        public string ToolTip => "Overlay info in the scene view (ownership, bandwidth)";
        public string ButtonText => "Open";
        public string DocumentationUrl => Doc.NetSceneVis;

#if UNITY_NETCODE_GAMEOBJECTS_1_1_ABOVE
        public bool IsAvailable => true;
        public string AvailabilityMessage => "Available";

        public void Open()
        {
            InteractedAnalyticHelper.Send(Name);
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
        public string AvailabilityMessage => "Network Scene Visualization is only available from version 2023.1.14f1, with Netcode for GameObjects 1.1+";
        public void Open() => throw new NotImplementedException();
#endif
    }
}
