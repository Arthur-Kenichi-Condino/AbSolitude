using System;
using UnityEditor;
using UnityEngine;

namespace Unity.Multiplayer.Tools.Editor.MultiplayerToolsWindow
{
    class NetworkSimulatorFeature : IMultiplayerToolsFeature
    {
        public string Name => "Network Simulator";
        public string ToolTip => "Enables to simulate adverse network conditions, including in a build";
        public string ButtonText => "Add to scene";
        public string DocumentationUrl => "https://docs-multiplayer.unity3d.com/tools/current/tools-network-simulator/";

#if UTP_TRANSPORT_2_0_ABOVE && UNITY_NETCODE_GAMEOBJECTS_1_1_ABOVE
        public bool IsAvailable => true;
        public string AvailabilityMessage => "Available";
        
        public void Open()
        {
            var go = new GameObject("Network Simulator");
            go.AddComponent<NetworkSimulator.Runtime.NetworkSimulator>();
            Undo.RegisterCreatedObjectUndo(go, "Add Network Simulator to scene");
            EditorGUIUtility.PingObject(go);
        }
#else
        public bool IsAvailable => false;
        public string AvailabilityMessage => "Network Simulator is only available with Unity 2022.2+, Unity Transport 2.0+ and with Netcode for GameObjects 1.1+";
        public void Open() => throw new NotImplementedException();
#endif
    }
}
