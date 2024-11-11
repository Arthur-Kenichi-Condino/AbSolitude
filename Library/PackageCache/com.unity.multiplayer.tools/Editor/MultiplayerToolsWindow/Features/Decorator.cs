using System;
using UnityEditor;

namespace Unity.Multiplayer.Tools.Editor.MultiplayerToolsWindow
{
    class Decorator : IMultiplayerToolsFeature
    {
        public string Name => "Hierarchy Network Debug View";
        
        public string ToolTip => "Add ownership info in the hierarchy view";

        public string DocumentationUrl => null;
        
#if UNITY_NETCODE_GAMEOBJECTS_1_1_ABOVE
        public bool IsAvailable => true;
        
        public string AvailabilityMessage => "Available";

        public string ButtonText => HierarchyWindowDecorator.Enabled ? "Disable" : "Enable";
        
        public void Open()
        {
            HierarchyWindowDecorator.Enabled = !HierarchyWindowDecorator.Enabled;
            EditorApplication.RepaintHierarchyWindow();
        }
#else
        public bool IsAvailable => false;
        public string AvailabilityMessage => "Hierarchy Network Debug View is only available with Netcode for GameObjects 1.1+";
        public string ButtonText => "Enable";
        public void Open() => throw new NotImplementedException();
#endif
    }
}
