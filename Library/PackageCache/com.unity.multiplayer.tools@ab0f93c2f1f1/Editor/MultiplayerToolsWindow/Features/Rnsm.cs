using System;
using Unity.Multiplayer.Tools.Common.Helpers;
using Unity.Multiplayer.Tools.Editor.MultiplayerToolsWindow.Analytics;
using Unity.Multiplayer.Tools.NetStatsMonitor;

namespace Unity.Multiplayer.Tools.Editor.MultiplayerToolsWindow
{
    class Rnsm : IMultiplayerToolsFeature
    {
        public string Name => "Runtime Network Stats Monitor";
        public string ToolTip => "The Runtime Network Stats Monitor enables you to monitor the bandwidth usage of your game at runtime.";
        public string ButtonText => "Add to Scene";
        public string DocumentationUrl => Doc.RuntimeNetStatsMonitor;

#if UNITY_NETCODE_GAMEOBJECTS_1_1_ABOVE
        public bool IsAvailable => true;
        public string AvailabilityMessage => "Available";

        public void Open()
        {
            InteractedAnalyticHelper.Send(Name);
            SceneToolInstance.SelectExistingOrCreate<RuntimeNetStatsMonitor>(
                "Runtime Network Stats Monitor",
                "This scene already has a Runtime Network Stats Monitor, so it has been selected instead of " +
                "adding another. Add the component manually to display a second monitor with its own " +
                "configuration and position.");
        }
#else
        public bool IsAvailable => false;
        public string AvailabilityMessage => "Runtime Network Stats Monitor is only available with Netcode for GameObjects 1.1+";
        public void Open() => throw new NotImplementedException();
#endif
    }
}
