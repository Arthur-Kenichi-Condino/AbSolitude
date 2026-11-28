using System;

namespace Unity.Multiplayer.Tools.Adapters
{
    /// <summary>
    /// Interface to get the number of RPCs called on an object this frame
    /// </summary>
    interface IGetRpcCount : IAdapterComponent
    {
        /// <exception cref="NoSubscribersException">
        /// may be thrown if this method is called without any subscribers to <see cref="OnRpcCountUpdated"/>.
        /// This is to ensure that object bandwidth is computed and stored only if required.
        /// </exception>
        int GetRpcCount(ObjectId objectId);

        /// <summary>
        /// Event that is triggered when new object RPC counts are available.
        /// If there are no subscribers, then RPC counts may not be computed and stored
        /// by the underlying implementation.
        /// </summary>
        event Action OnRpcCountUpdated;
    }
}
