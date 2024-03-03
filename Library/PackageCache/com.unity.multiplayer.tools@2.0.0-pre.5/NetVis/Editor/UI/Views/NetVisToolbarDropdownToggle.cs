using System.Threading.Tasks;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.NetVis.Configuration;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetVis.Editor.UI
{
    abstract class NetVisToolbarDropdownToggle : EditorToolbarDropdownToggle
    {
        protected NetVisToolbarDropdownToggle(string name, string tooltip, NetVisIcon icon, string text = "")
        {
            this.name = name;
            this.tooltip = tooltip;

            var editorTheme = EditorGUIUtility.isProSkin ? EditorTheme.Dark : EditorTheme.Light;
            LoadIcon(icon, editorTheme).Forget();

            this.text = text;

            ConfigurationWithEvents = PresentationContext.Instance.ConfigurationWithEvents;
            ConfigurationWithEvents.MetricChanged += OnMetricChanged;
            OnMetricChanged(ConfigurationWithEvents.Metric);
            dropdownClicked += ShowOverlayPopUp;
            this.RegisterValueChangedCallback(OnStateChange);
        }

        async Task LoadIcon(NetVisIcon icon, EditorTheme editorTheme)
        {
            this.icon = await icon.LoadAsync(editorTheme);
        }

        protected NetVisConfigurationWithEvents ConfigurationWithEvents { get; }

        protected abstract void OnMetricChanged(NetVisMetric metric);

        protected abstract void OnStateChange(ChangeEvent<bool> stateChange);

        protected abstract void ShowOverlayPopUp();
    }
}