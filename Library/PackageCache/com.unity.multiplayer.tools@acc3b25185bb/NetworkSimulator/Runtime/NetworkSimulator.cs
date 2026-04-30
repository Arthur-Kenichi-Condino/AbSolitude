using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Multiplayer.Tools.Adapters;
using Unity.Multiplayer.Tools.Common.Helpers;
using UnityEngine;
#if UNITY_EDITOR && UNITY_2023_2_OR_NEWER
using UnityEditor;
using Unity.Multiplayer.Tools.NetworkSimulator.Runtime.Analytics;
#endif

namespace Unity.Multiplayer.Tools.NetworkSimulator.Runtime
{
    /// <summary>
    /// Add this component to any game object to configure network simulation parameters.
    /// </summary>
    [AddComponentMenu("Netcode/Network Simulator")]
    [HelpURL(Doc.NetworkSimulator)]
    public partial class NetworkSimulator : MonoBehaviour, INotifyPropertyChanged
    {
        /// <summary>
        /// Enum for representing the current state of the scenario playback.
        /// </summary>
        internal enum ScenarioPlaybackState
        {
            Initial,    // The scenario has not been started yet
            Running,
            Paused
        }

        void OnPauseStateChangedEvent(bool isPaused)
        {
            scenarioPlaybackState = isPaused ? ScenarioPlaybackState.Paused : ScenarioPlaybackState.Running;
        }

        [SerializeField]
        internal NetworkSimulatorPresetAsset m_PresetAsset;

        [SerializeReference, HideInInspector]
        internal INetworkSimulatorPreset m_PresetReference = new NetworkSimulatorPreset();

        [SerializeReference]
        internal NetworkScenario m_Scenario;

        [SerializeField, HideInInspector]
        internal bool m_IsScenarioSettingsFolded;

        /// <summary>
        /// Allows to determine if network scenarios should start automatically or not.
        /// </summary>
        [SerializeField]
        public bool AutoRunScenario;

        readonly INetworkTransportApi m_NetworkTransportApi = new NetworkTransportApi();
        INetworkEventsApi m_NetworkEventsApi;
        INetworkSimulatorPreset m_CachedPreset;
        bool m_CachedScenarioIsPaused;
        ScenarioPlaybackState m_ScenarioPlaybackState = ScenarioPlaybackState.Initial;

        // Analytics
        internal bool UsedEditorGUI;
        private string m_LastPresetName;

        internal PropertyChangedEventHandler m_PropertyChanged;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add => m_PropertyChanged += value;
            remove => m_PropertyChanged -= value;
        }

        internal INetworkEventsApi NetworkEventsApi
        {
            get
            {
                if (m_NetworkEventsApi != null)
                {
                    return m_NetworkEventsApi;
                }
                m_NetworkEventsApi = new NetworkEventsApi(this, m_NetworkTransportApi);
                return m_NetworkEventsApi;
            }
        }

        /// <summary>
        /// Returns whether the underlying network transport is connected.
        /// </summary>
        public bool IsConnected => NetworkEventsApi.IsConnected;

        /// <summary>
        /// Returns whether the underlying network transport is available.
        /// </summary>
        public bool IsAvailable => NetworkEventsApi.IsAvailable;

        /// <summary>
        /// The Connection Preset used to define a set of connection parameters to simulate the network condition at runtime.
        /// </summary>
        public INetworkSimulatorPreset ConnectionPreset
        {
            get => m_PresetAsset != null
                ? m_PresetAsset
                : m_PresetReference;
            set
            {
                if (value is NetworkSimulatorPresetAsset presetAsset)
                {
                    m_PresetAsset = presetAsset;
                    m_PresetReference = null;
                }
                else
                {
                    m_PresetReference = value;
                    m_PresetAsset = null;
                }

                UpdateLiveParameters();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Delegate for handling scenario changes.
        /// </summary>
        internal delegate void ScenarioChangedHandler(NetworkScenario newScenario);

        /// <summary>
        /// Event triggered when the active scenario is changed.
        /// </summary>
        internal event ScenarioChangedHandler ScenarioChangedEvent = delegate { };

        /// <summary>
        /// The Network Scenario used to modify network connection parameters at runtime.
        /// The new scenario will start automatically under specific circumstances:
        /// If autorun is enabled and
        /// the application is in playmode and
        /// the new scenario is valid (not None/null) and
        /// the previous scenario was a running valid one.
        /// </summary>
        public NetworkScenario Scenario
        {
            get => m_Scenario;
            set
            {
                if (Equals(m_Scenario, value))
                {
                    return;
                }

                var previousValue = Scenario;

                if (previousValue != null)
                {
                    previousValue.PauseStateChangedEvent -= OnPauseStateChangedEvent;
                }

                m_Scenario = value;

                if (m_Scenario != null)
                {
                    m_Scenario.PauseStateChangedEvent += OnPauseStateChangedEvent;
                }

                scenarioPlaybackState = ScenarioPlaybackState.Initial;

                if (Application.isPlaying)
                {
                    previousValue?.Dispose();

                    if (m_Scenario != null)
                    {
                        var shouldAutoRun = AutoRunScenario && previousValue != null && !previousValue.IsPaused;
                        m_Scenario.InitializeScenario(NetworkEventsApi, shouldAutoRun);
                    }
                }

                ScenarioChangedEvent(m_Scenario);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Delegate for handling scenario playback state changes.
        /// </summary>
        internal delegate void ScenarioPlaybackStateChangedHandler(ScenarioPlaybackState newState);

        /// <summary>
        /// Event triggered when the scenario playback state changes.
        /// </summary>
        internal event ScenarioPlaybackStateChangedHandler ScenarioPlaybackStateChangedEvent = delegate { };

        /// <summary>
        /// This state represents the state of the current scenario being used by the simulator.
        /// </summary>
        internal ScenarioPlaybackState scenarioPlaybackState
        {
            get => m_ScenarioPlaybackState;
            private set
            {
                if (m_ScenarioPlaybackState == value)
                {
                    return;
                }

                m_ScenarioPlaybackState = value;
                ScenarioPlaybackStateChangedEvent(scenarioPlaybackState);
            }
        }

        internal void UpdateLiveParameters(bool forceUpdate = false)
        {
            if (forceUpdate == false && enabled == false)
            {
                return;
            }

            Analytic(forceUpdate);

            m_NetworkTransportApi.UpdateNetworkParameters(
                new()
                {
                    PacketDelayMilliseconds = ConnectionPreset?.PacketDelayMs ?? 0,
                    PacketDelayRangeMilliseconds = ConnectionPreset?.PacketJitterMs ?? 0,
                    PacketLossIntervalMilliseconds = ConnectionPreset?.PacketLossInterval ?? 0,
                    PacketLossPercent = ConnectionPreset?.PacketLossPercent ?? 0
                });
        }

        private void Analytic(bool forceUpdate)
        {
#if UNITY_EDITOR && UNITY_2023_2_OR_NEWER
            // Track when a connection preset is changed to a preset, only once per changed.
            if (forceUpdate == false &&
                m_LastPresetName != ConnectionPreset?.Name &&
                ConnectionPreset?.Name != "None")
            {
                m_LastPresetName = ConnectionPreset?.Name;
                var presetName = m_LastPresetName;
                var isPartOfScenario = scenarioPlaybackState == ScenarioPlaybackState.Running;
                // Obfuscate custom preset name to not gather sensitive data
                if (!NetworkSimulatorPresets.Names.Contains(presetName))
                {
                    presetName = "Custom";
                }

                EditorAnalytics.SendAnalytic(new ConnectionPresetChangedAnalytic(UsedEditorGUI, presetName, isPartOfScenario));
            }
#endif
        }

        void SetUsedEditorReset()
        {
#if UNITY_EDITOR && UNITY_2023_2_OR_NEWER
            // Track editor GUI usage per playmode session for analytics
            EditorApplication.playModeStateChanged += stateChange =>
            {
                if (stateChange == PlayModeStateChange.EnteredPlayMode)
                {
                    UsedEditorGUI = false;
                }
            };
#endif
        }

        void OnEnable()
        {

            SetUsedEditorReset();

            if (m_CachedPreset != null)
            {
                ConnectionPreset = m_CachedPreset;
            }

            if (Scenario != null)
            {
                Scenario.PauseStateChangedEvent += OnPauseStateChangedEvent;
                Scenario.IsPaused = m_CachedScenarioIsPaused;
            }

            UpdateLiveParameters();
            NetworkAdapters.OnAdapterAdded += NetworkAdapterAdded;
        }

        void NetworkAdapterAdded(INetworkAdapter obj)
        {
            UpdateLiveParameters();
        }

        void Start()
        {
            scenarioPlaybackState = ScenarioPlaybackState.Initial;
            Scenario?.InitializeScenario(NetworkEventsApi, AutoRunScenario);
        }

        void OnDisable()
        {
            m_CachedPreset = ConnectionPreset;
            ConnectionPreset = NetworkSimulatorPresets.None;
            UpdateLiveParameters(true);

            if (Scenario != null)
            {
                Scenario.PauseStateChangedEvent -= OnPauseStateChangedEvent;
            }

            NetworkAdapters.OnAdapterAdded -= NetworkAdapterAdded;
        }

        void OnDestroy()
        {
            Scenario?.Dispose();
        }

        void Update()
        {
            if (Scenario is NetworkScenarioBehaviour scenarioBehaviour)
            {
                scenarioBehaviour.UpdateScenario(Time.deltaTime);
            }
        }

        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            m_PropertyChanged?.Invoke(this, new(propertyName));
        }
    }
}
