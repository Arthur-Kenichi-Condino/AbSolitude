using System;
using System.Threading.Tasks;

namespace Unity.Multiplayer.Tools.NetworkSimulator.Runtime
{
    // NetworkEvents Api Proxy
    public partial class NetworkSimulator : INetworkEventsApi
    {
        /// <inheritdoc />
        public void Disconnect() => NetworkEventsApi.Disconnect();

        /// <inheritdoc />
        public void Reconnect() => NetworkEventsApi.Reconnect();

        /// <inheritdoc />
        public void TriggerLagSpike(TimeSpan duration) => NetworkEventsApi.TriggerLagSpike(duration);

        /// <inheritdoc />
        public Task TriggerLagSpikeAsync(TimeSpan duration) => NetworkEventsApi.TriggerLagSpikeAsync(duration);
        /// <inheritdoc />
        public void ChangeConnectionPreset(INetworkSimulatorPreset preset) => NetworkEventsApi.ChangeConnectionPreset(preset);

        /// <inheritdoc />
        public INetworkSimulatorPreset CurrentPreset => NetworkEventsApi.CurrentPreset;
    }
}
