using System.Collections.Generic;
using Unity.Multiplayer.Tools.Common;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetworkSimulator.Editor.UI
{
    class NetworkEventsView : VisualElement
    {
        const string k_Uxml = "Packages/com.unity.multiplayer.tools/NetworkSimulator/Editor/UI/NetworkEventsView.uxml";

        const string k_ToggleConnectedString = "Connected";
        const string k_ToggleDisconnectedString = "Disconnected";
        const string k_DisconnectedClassName = "disconnected";

        ToggleButtonStrip ConnectionToggle => this.Q<ToggleButtonStrip>(nameof(ConnectionToggle));
        VisualElement ConnectionIndicator => this.Q<VisualElement>(nameof(ConnectionIndicator));

        readonly Runtime.NetworkSimulator m_NetworkSimulator;

        internal NetworkEventsView(Runtime.NetworkSimulator networkSimulator)
        {
            m_NetworkSimulator = networkSimulator;

            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_Uxml).CloneTree(this);
            ConnectionToggle.onButtonClick += delegate
            {
                OnConnectionStateTogglePressed(null);
            };
            ConnectionToggle.choices = new List<string> { k_ToggleConnectedString, k_ToggleDisconnectedString };

            this.AddEventLifecycle(OnAttach, OnDetached);
        }

        void OnAttach(AttachToPanelEvent evt)
        {
            ConnectionToggle.RegisterCallback<MouseUpEvent>(OnConnectionStateTogglePressed);
            EditorApplication.update += OnEditorUpdate;
        }

        void OnDetached(DetachFromPanelEvent evt)
        {
            ConnectionToggle.UnregisterCallback<MouseUpEvent>(OnConnectionStateTogglePressed);

            EditorApplication.update -= OnEditorUpdate;
        }

        void OnConnectionStateTogglePressed(MouseUpEvent _)
        {
            m_NetworkSimulator.UsedEditorGUI = true;
            if (ConnectionToggle.value == k_ToggleConnectedString)
            {
                m_NetworkSimulator.Reconnect();
            }
            else
            {
                m_NetworkSimulator.Disconnect();
            }
        }

        void OnEditorUpdate()
        {
            ConnectionToggle.SetValueWithoutNotify(m_NetworkSimulator.IsConnected
                ? k_ToggleConnectedString
                : k_ToggleDisconnectedString);

            ConnectionIndicator.EnableInClassList(k_DisconnectedClassName, !m_NetworkSimulator.IsConnected);
        }
    }
}
