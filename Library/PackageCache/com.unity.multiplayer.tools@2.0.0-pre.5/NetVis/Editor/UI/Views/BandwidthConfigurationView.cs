using System;
using System.Linq;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.DependencyInjection;
using Unity.Multiplayer.Tools.DependencyInjection.UIElements;
using Unity.Multiplayer.Tools.NetVis.Configuration;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetVis.Editor.UI
{
    [LoadUxmlView(NetVisEditorPaths.k_UxmlRoot)]
    class BandwidthConfigurationView : InjectedVisualElement<BandwidthConfigurationView>
    {
        static readonly BandwidthTypes[] k_BandwidthTypeChoices =
        {
            BandwidthTypes.All,
            BandwidthTypes.NetVar,
            BandwidthTypes.Rpc,
        };

        static readonly NetworkDirection[] k_NetworkDirectionChoices =
        {
            NetworkDirection.SentAndReceived,
            NetworkDirection.Sent,
            NetworkDirection.Received
        };

        // Filtering
        // -------------------------------------------------------------------------------------------------------------
        [UxmlQuery] DropdownField BandwidthType;
        [UxmlQuery(Name = nameof(BandwidthSettings.NetworkDirection))] DropdownField NetworkDirectionField;

        // Mesh shading
        // -------------------------------------------------------------------------------------------------------------
        [UxmlQuery] Toggle MeshShadingEnabled;
        [UxmlQuery] Slider Smoothing;

        // Text overlay
        // -------------------------------------------------------------------------------------------------------------
        [UxmlQuery] Toggle TextOverlayEnabled;

        [Inject] NetVisConfigurationWithEvents Configuration;
        BandwidthSettings Settings => Configuration.Configuration.Settings.Bandwidth;

        [Inject] IReadonlyBandwidthStats BandwidthStats;

        public BandwidthConfigurationView()
        {
            // Filtering
            // ---------------------------------------------------------------------------------------------------------
            BandwidthType.choices = k_BandwidthTypeChoices
                .Select(bandwidthType => bandwidthType.DisplayName())
                .ToList();
            BandwidthType.Bind(
                BandwidthType.choices[Array.IndexOf(k_BandwidthTypeChoices, Settings.BandwidthType)],
                _ =>
            {
                Settings.BandwidthType = k_BandwidthTypeChoices[BandwidthType.index];
                Configuration.NotifySettingsChanged();
            });

            NetworkDirectionField.choices = k_NetworkDirectionChoices
                .Select(direction => direction.DisplayName())
                .ToList();
            NetworkDirectionField.Bind(
                NetworkDirectionField.choices[Array.IndexOf(k_NetworkDirectionChoices, Settings.NetworkDirection)],
                _ =>
            {
                Settings.NetworkDirection = k_NetworkDirectionChoices[NetworkDirectionField.index];
                Configuration.NotifySettingsChanged();
            });

            // Mesh shading
            // -------------------------------------------------------------------------------------------------------------
            MeshShadingEnabled.Bind(
                Settings.MeshShadingEnabled,
                value =>
                {
                    Settings.MeshShadingEnabled = value;
                    Configuration.NotifySettingsChanged();
                });

            Smoothing.Bind(Settings.SmoothingHalfLife, value =>
            {
                Settings.SmoothingHalfLife = value;
                Configuration.NotifySettingsChanged();
            });

            // Text overlay
            // -------------------------------------------------------------------------------------------------------------
            TextOverlayEnabled.Bind(
                Settings.TextOverlayEnabled,
                value =>
                {
                    Settings.TextOverlayEnabled = value;
                    Configuration.NotifySettingsChanged();
                });
        }

        public new class UxmlFactory : UxmlFactory<BandwidthConfigurationView, UxmlTraits> { }
    }
}
