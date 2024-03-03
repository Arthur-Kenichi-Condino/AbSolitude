using System;
using System.Collections.Generic;

namespace Unity.Multiplayer.Tools.Adapters
{
    interface IGetConnectedClients : IAdapterComponent
    {
        IReadOnlyList<ClientId> ConnectedClients { get; }
        event Action<ClientId> ClientConnectionEvent;
        event Action<ClientId> ClientDisconnectionEvent;
    }
}