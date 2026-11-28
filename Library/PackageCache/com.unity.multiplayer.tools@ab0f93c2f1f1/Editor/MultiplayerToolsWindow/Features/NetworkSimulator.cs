using System;
using Unity.Multiplayer.Tools.Common.Helpers;
using Unity.Multiplayer.Tools.Editor.MultiplayerToolsWindow.Analytics;

namespace Unity.Multiplayer.Tools.Editor.MultiplayerToolsWindow
{
    class NetworkSimulatorFeature : IMultiplayerToolsFeature
    {
        public string Name => "Network Simulator";
        public string ToolTip => "Enables to simulate adverse network conditions, including in a build";
        public string ButtonText => "Add to Scene";
        public string DocumentationUrl => Doc.NetworkSimulator;

#if UTP_TRANSPORT_2_0_ABOVE && UNITY_NETCODE_GAMEOBJECTS_1_1_ABOVE
        public bool IsAvailable => true;
        public string AvailabilityMessage => "Available";
        
        public void Open()
        {
            InteractedAnalyticHelper.Send(Name);
            SceneToolInstance.SelectExistingOrCreate<NetworkSimulator.Runtime.NetworkSimulator>(
                "Network Simulator",
                "This scene already has a Network Simulator, so it has been selected instead of adding " +
                "another. Every Network Simulator drives the same network transport, so only one should be " +
                "enabled at a time.");
        }
#else
        public bool IsAvailable => false;
        public string AvailabilityMessage => "Network Simulator is only available with Unity 2022.2+, Unity Transport 2.0+ and with Netcode for GameObjects 1.1+";
        public void Open() => throw new NotImplementedException();
#endif
    }
}
