using System;
using Unity.Multiplayer.Tools.NetStatsMonitor;
using UnityEditor;
using UnityEngine;

namespace Unity.Multiplayer.Tools.Editor.MultiplayerToolsWindow
{
    class Rnsm : IMultiplayerToolsFeature
    {
        public string Name => "Runtime Network Stats Monitor";
        public string ToolTip => "The Runtime Network Stats Monitor enables you to monitor the bandwidth usage of your game at runtime.";
        public string ButtonText => "Add to scene";
        public string DocumentationUrl => "https://docs-multiplayer.unity3d.com/tools/current/RNSM/";
        
#if UNITY_NETCODE_GAMEOBJECTS_1_1_ABOVE
        public bool IsAvailable => true;
        public string AvailabilityMessage => "Available";

        public void Open()
        {
            var go = new GameObject("Runtime Network Stats Monitor");
            go.AddComponent<RuntimeNetStatsMonitor>();
            Undo.RegisterCreatedObjectUndo(go, "Add Runtime Network Stats Monitor to scene");
            EditorGUIUtility.PingObject(go);
        }
#else
        public bool IsAvailable => false;
        public string AvailabilityMessage => "Runtime Network Stats Monitor is only available with Netcode for GameObjects 1.1+";
        public void Open() => throw new NotImplementedException();
#endif
    }
}
