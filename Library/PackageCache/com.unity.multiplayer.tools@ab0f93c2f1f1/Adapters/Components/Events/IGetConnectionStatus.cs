using System;

namespace Unity.Multiplayer.Tools.Adapters
{
    interface IGetConnectionStatus : IAdapterComponent
    {
        event Action ServerOrClientStarted;
        event Action ServerOrClientStopped;
    }
}
