using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Multiplayer.Tools.Adapters;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.DependencyInjection;
using Unity.Multiplayer.Tools.DependencyInjection.UIElements;
using Unity.Multiplayer.Tools.NetVis.Configuration;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetVis.Editor.UI
{
#if UNITY_2023_3_OR_NEWER
    [UxmlElement]
#endif
    [LoadUxmlView(NetVisEditorPaths.k_UxmlRoot)]
    partial class OwnershipServerClientConfigurationView : InjectedVisualElement<OwnershipServerClientConfigurationView>
    {
        const bool k_ClientColorShowAlpha = false;
        const bool k_ClientColorShowEyeDropper = true;

        [UxmlQuery]
        Label ServerHost;
        [UxmlQuery]
        ColorField ServerHostColor;
        [UxmlQuery]
        Label NoClientsConnectedText;
        [UxmlQuery]
        VisualElement ClientsContainer;

        [Inject]
        NetVisConfigurationWithEvents Configuration;
        [Inject]
        IGetConnectedClients ConnectedClientsRepository;
        OwnershipSettings OwnershipSettings => Configuration.Configuration.Settings.Ownership;

        readonly List<(ClientId ClientId, ColorField Color, Action<NetVisSettings> SettingsChangedCallback, Action ColorsChangedCallback)> m_ConnectedClients = new();

        public OwnershipServerClientConfigurationView()
        {
            this.AddEventLifecycle(OnAttach, OnDetach);

            ServerHostColor.showAlpha = k_ClientColorShowAlpha;
            ServerHostColor.showEyeDropper = k_ClientColorShowEyeDropper;

            ServerHostColor.Bind(OwnershipSettings.ServerHostColor,
                newColor => OwnershipSettings.SetCustomColor(0, newColor));

            this.AddManipulator(new ContextualMenuManipulator(evt =>
                evt.menu.AppendAction("Reset Colors", _ =>
                {
                    OwnershipSettings.ResetCustomColors();
                })));

            Configuration.SettingsChanged += _ => RefreshColorField(ServerHostColor, OwnershipSettings.ServerHostColor);
            OwnershipSettings.ColorsChanged += () => RefreshColorField(ServerHostColor, OwnershipSettings.ServerHostColor);

            RefreshClientList();
        }

        void RefreshClientList()
        {
            var clientIdsToRemove = m_ConnectedClients.Select(c => c.ClientId).ToList();
            foreach (var clientId in clientIdsToRemove)
            {
                RemoveClient(clientId);
            }

            foreach (var client in ConnectedClientsRepository.ConnectedClients)
            {
                AddClient(client);
            }
        }

        void OnAttach(AttachToPanelEvent _)
        {
            // There could have been client changes while we were detached
            RefreshClientList();

            ConnectedClientsRepository.ClientConnectionEvent += AddClient;
            ConnectedClientsRepository.ClientDisconnectionEvent += RemoveClient;
        }

        void OnDetach(DetachFromPanelEvent _)
        {
            ConnectedClientsRepository.ClientConnectionEvent -= AddClient;
            ConnectedClientsRepository.ClientDisconnectionEvent -= RemoveClient;
        }

        void AddClient(ClientId clientId)
        {
            // We don't want to display the host in the clients list
            if (clientId == 0)
            {
                return;
            }

            if (m_ConnectedClients.Exists(entry => entry.ClientId == clientId))
            {
                return;
            }

            var colorField = new ColorField
            {
                label = clientId.ToString(),
                showAlpha = k_ClientColorShowAlpha,
                showEyeDropper = k_ClientColorShowEyeDropper
            };

            var clientColor = OwnershipSettings.GetClientColor(clientId);
            colorField.Bind(clientColor, newColor => OwnershipSettings.SetCustomColor(clientId, newColor));

            Action<NetVisSettings> settingsChanged = _ => RefreshColorField(colorField, OwnershipSettings.GetClientColor(clientId));
            Configuration.SettingsChanged += settingsChanged;

            Action colorsChanged = () => RefreshColorField(colorField, OwnershipSettings.GetClientColor(clientId));
            OwnershipSettings.ColorsChanged += colorsChanged;

            m_ConnectedClients.Add((clientId, colorField, settingsChanged, colorsChanged));
            ClientsContainer.Add(colorField);

            NoClientsConnectedText.IncludeInLayout(false);
        }

        static void RefreshColorField(ColorField colorField, Color color)
        {
            colorField.SetValueWithoutNotify(color);
            colorField.showAlpha = k_ClientColorShowAlpha;
            colorField.showEyeDropper = k_ClientColorShowEyeDropper;
            colorField.SetEnabled(true);
        }

        void RemoveClient(ClientId clientId)
        {
            var client = m_ConnectedClients.FirstOrDefault(x => x.ClientId == clientId);
            if (client != default)
            {
                Configuration.SettingsChanged -= client.SettingsChangedCallback;
                OwnershipSettings.ColorsChanged -= client.ColorsChangedCallback;

                ClientsContainer.Remove(client.Color);
                m_ConnectedClients.Remove(client);
            }

            if (!m_ConnectedClients.Any())
            {
                NoClientsConnectedText.IncludeInLayout(true);
            }
        }

        static void DisableWithoutChangingColor(ColorField colorField)
        {
            // Normally disabling a UI Element partially desaturates of greys-out a UI Element.
            // In the case of our colors, this is not desirable as it will change the colors shown in the legend
            // and mean that they will no longer match the colors that are rendered. To avoid this problem we
            // disable, but then manually remove the `disableUssClassName` that would result in this styling change
            colorField.SetEnabled(false);
            colorField.RemoveFromClassList(disabledUssClassName);
        }

#if !UNITY_2023_3_OR_NEWER
        public new class UxmlFactory : UxmlFactory<OwnershipServerClientConfigurationView, UxmlTraits> { }
#endif
    }
}
