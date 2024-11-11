using System.Linq;
using JetBrains.Annotations;
using Unity.Multiplayer.Tools.NetworkSimulator.Runtime;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetworkSimulator.Editor.UI
{
#if UNITY_2023_3_OR_NEWER
    [UxmlElement]
#endif
    partial class NetworkPresetDropdown : DropdownField
    {
#if !UNITY_2023_3_OR_NEWER
        public new class UxmlFactory : UxmlFactory<NetworkPresetDropdown, UxmlTraits> { }
#endif

        public const string Custom = nameof(Custom);

        public NetworkPresetDropdown()
        {
            var presets = NetworkSimulatorPresets.Values.Select(x => x.Name).ToList();
            presets.Add(Custom);
            choices = presets;
        }

        int IndexOf(string choice)
        {
            return choices.IndexOf(choice);
        }

        internal void UpdatePresetDropdown([CanBeNull]string configurationName)
        {
            configurationName = string.IsNullOrEmpty(configurationName)
                ? Custom
                : configurationName;

            var newIndex = IndexOf(configurationName);
            index = newIndex == -1
                ? IndexOf(Custom)
                : newIndex;
        }
    }
}
