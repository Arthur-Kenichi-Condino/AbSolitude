using System;
using UnityEditor;

namespace Unity.Multiplayer.Tools.Editor.MultiplayerToolsWindow
{
    class NetworkProfiler : IMultiplayerToolsFeature
    {
        public string Name => "Network Profiler";

        public string ToolTip => "The network profiler modules enables you to analyze the bandwidth usage of your game.";

        public string ButtonText => "Open";

        public string DocumentationUrl => "https://docs-multiplayer.unity3d.com/tools/current/profiler/";

#if UNITY_NETCODE_GAMEOBJECTS_1_1_ABOVE
        public bool IsAvailable => true;
        public string AvailabilityMessage => "Available";
        
        public void Open()
        {
            EditorWindow.GetWindow<ProfilerWindow>();
            //TODO: did not find how to load the modules via code. Ask around?
        }
#else
        public bool IsAvailable => false;
        public string AvailabilityMessage => "Network Profiler is only available with Netcode for GameObjects 1.1+";
        public void Open() => throw new NotImplementedException();
#endif
    }
}
