using System;

namespace Unity.Multiplayer.Tools.NetVis.Configuration
{
    class NetVisConfigurationWithEvents
    {
        NetVisConfiguration m_Configuration;

        public event Action<NetVisConfiguration> ConfigurationChanged;
        public event Action<NetVisMetric> MetricChanged;
        public event Action<NetVisSettings> SettingsChanged;

        public NetVisConfigurationWithEvents(NetVisConfiguration configuration)
        {
            m_Configuration = configuration;
        }

        public NetVisConfiguration Configuration
        {
            get => m_Configuration;
            set
            {
                if (value == m_Configuration)
                {
                    return;
                }

                var oldConfig = m_Configuration;
                m_Configuration = value;
                if (m_Configuration.Metric != oldConfig.Metric)
                {
                    MetricChanged?.Invoke(m_Configuration.Metric);
                }

                if (m_Configuration.Settings != oldConfig.Settings)
                {
                    SettingsChanged?.Invoke(m_Configuration.Settings);
                }

                ConfigurationChanged?.Invoke(m_Configuration);
            }
        }

        public NetVisMetric Metric
        {
            get => Configuration.Metric;
            set
            {
                if (value == Configuration.Metric)
                {
                    return;
                }

                Configuration.Metric = value;
                MetricChanged?.Invoke(m_Configuration.Metric);
                ConfigurationChanged?.Invoke(m_Configuration);
            }
        }

        /// <summary>
        /// Notify listeners that the settings have changed.
        /// Also triggers the ConfigurationChanged event.
        /// </summary>
        public void NotifySettingsChanged()
        {
            SettingsChanged?.Invoke(m_Configuration.Settings);
            ConfigurationChanged?.Invoke(m_Configuration);
        }
    }
}
