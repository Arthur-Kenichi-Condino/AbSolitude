using System;
using JetBrains.Annotations;
using Unity.Multiplayer.Tools.NetStats;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Multiplayer.Tools.NetStatsMonitor.Implementation;

namespace Unity.Multiplayer.Tools.NetStatsMonitor
{
    /// <summary>
    /// The Runtime Net Stats Monitor component.
    /// Add this component to a game object in a scene to display network statistics on screen.
    /// </summary>
    [AddComponentMenu("Netcode/Runtime Network Stats Monitor")]
    public class RuntimeNetStatsMonitor : MonoBehaviour
    {
        /// Visibility toggle to hide or show the on-screen display.
        [SerializeField]
        [Tooltip("Visibility toggle to hide or show the on-screen display.")]
        bool m_Visible = true;

        /// <summary>
        /// Visibility toggle to hide or show the on-screen display.
        /// </summary>
        public bool Visible
        {
            get => m_Visible;
            set
            {
                m_Visible = value;
                UpdateUiVisibility();
            }
        }

        /// <summary>
        /// The maximum rate at which the Runtime Net Stats Monitor's on-screen display is updated (per second).
        /// The on-screen display will never be updated faster than the overall refresh rate.
        /// The default refresh rate is 30fps.
        /// </summary>
        public double MaxRefreshRate
        {
            get => m_MaxRefreshRate;
            set => m_MaxRefreshRate = Math.Max(value, ConfigurationLimits.k_RefreshRateMin);
        }

        /// The maximum rate at which the Runtime Net Stats Monitor's on-screen display is updated (per second).
        /// The on-screen display will never be updated faster than the overall refresh rate.
        /// The default refresh rate is 30fps.
        [SerializeField]
        [Min((float)ConfigurationLimits.k_RefreshRateMin)]
        [Tooltip("The maximum rate at which the Runtime Net Stats Monitor's on-screen display is updated " +
            "(per second). " +
            "The on-screen display will never be updated faster than the overall refresh rate.")]
        double m_MaxRefreshRate = 30;

        /// <summary>
        /// Custom stylesheet to override the default style of the Runtime Net Stats Monitor.
        /// </summary>
        [field:SerializeField]
        public StyleSheet CustomStyleSheet { get; set; }

        /// <summary>
        /// Optional panel settings that can be used to override the default.
        /// These panel settings can be used to control a number of things, including how the on-screen display
        /// of the Runtime Net Stats Monitor scales on different devices and displays.
        /// </summary>
        [field:Tooltip(
            "Optional panel settings that can be used to override the default. " +
            "These panel settings can be used to control a number of things, including how the on-screen display " +
            "of the Runtime Net Stats Monitor scales on different devices and displays. ")]
        [field:SerializeField]
        public PanelSettings PanelSettingsOverride { get; set; }

        /// <summary>
        /// Position configuration that allows custom positioning on screen
        /// The default position is the top left corner of the screen
        /// </summary>
        [field: SerializeField]
        public PositionConfiguration Position { get; set; } = new();

        /// <summary>
        /// The configuration asset used to configure the information displayed in this Runtime Net Stats Monitor.
        /// The NetStatsMonitorConfiguration can created from the Create menu, or from C# using
        /// ScriptableObject.CreateInstance.
        /// </summary>
        [CanBeNull]
        [field: SerializeField]
        [field: Tooltip(
            "The configuration asset used to configure the information displayed in this Runtime Net Stats Monitor. " +
            "The NetStatsMonitorConfiguration can created from the Create menu, or from C# using " +
            "ScriptableObject.CreateInstance."
        )]
        public NetStatsMonitorConfiguration Configuration { get; set; }

        [CanBeNull]
        internal RnsmComponentImplementation Implementation { get; private set; }

        void Start()
        {
            Setup();
        }

        void OnEnable()
        {
            Setup();
        }

        void OnDisable()
        {
            Teardown();
        }

        void OnDestroy()
        {
            Teardown();
        }

        void OnValidate()
        {
            if (enabled)
            {
                ApplyConfiguration();
            }
            else
            {
                Teardown();
            }
        }

        /// Perform any remaining setup steps and setup any missing fields, components, or events,
        /// but do not overwrite fields or components that are already set up
        internal void Setup()
        {
            SetupImplementation();
            UpdateUiVisibility();
        }

        /// Teardown any fields/components that are not needed while this component is disabled
        /// to reduce resource usage
        internal void Teardown()
        {
            PerformRemainingImplementationTeardownSteps();
        }

        /// <summary>
        /// Apply the CustomStyleSheet, Position, and Configuration to the monitor.
        /// This function must be called when these fields have been modified from C#
        /// in order to apply the changes. This function does not need to be called
        /// when these fields are modified in the inspector, as changes made in the
        /// inspector are detected and applied automatically
        /// </summary>
        public void ApplyConfiguration()
        {
            if (Configuration != null)
            {
                Configuration.RecomputeConfigurationHash();
            }
            ConfigureImplementation();
            UpdateUiVisibility();
        }

        /// <summary>
        /// Add a custom value for this metricId, which can be displayed in the
        /// RuntimeNetStatsMonitor using a counter or graph configured to display
        /// this metric.
        /// </summary>
        /// <param name="metricId">The custom <see cref="MetricId"/> to provide a value for.</param>
        /// <param name="value">The value of the metric.</param>
        public void AddCustomValue(MetricId metricId, float value)
        {
            Implementation?.AddCustomValue(metricId, value);
        }

        void UpdateUiVisibility()
        {
            Implementation?.UpdateUiVisibility(enabled, m_Visible);
        }

        void SetupImplementation()
        {
            Implementation ??= new RnsmComponentImplementation();
            Implementation?.SetupAndConfigure(Configuration, Position, CustomStyleSheet, PanelSettingsOverride, MaxRefreshRate);
        }

        void ConfigureImplementation()
        {
            Implementation?.Configure(Configuration, Position, CustomStyleSheet, PanelSettingsOverride);
        }

        void PerformRemainingImplementationTeardownSteps()
        {
            Implementation?.Teardown();
            Implementation = null;
        }

        internal void Update()
        {
            if (!Visible)
            {
                return;
            }
            Implementation?.Update(Configuration, MaxRefreshRate);
        }
    }
}
