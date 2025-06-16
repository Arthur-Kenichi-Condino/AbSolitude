using System;
using Unity.Multiplayer.Tools.Common;

namespace Unity.Multiplayer.Tools.Adapters
{
    /// <summary>
    /// Interface to get the bandwidth in bytes of an object this frame
    /// </summary>
    interface IGetBandwidth : IAdapterComponent
    {
        /// <summary>
        /// Returns the types of bandwidth that are supported by this adapter
        /// </summary>
         BandwidthTypes SupportedBandwidthTypes { get; }

        /// <summary>
        /// Returns the amount of bandwidth related to this object, filtered by bandwidth type and direction.
        /// </summary>
        /// <remarks>
        /// If any of the bandwidth type flags are not supported by this adapter then they will be ignored
        /// and will not contribute to the total bandwidth returned. If none of the bandwidth flags are
        /// supported by this adapter then the bandwidth returned will be zero.
        /// </remarks>
        /// <exception cref="NoSubscribersException">
        /// may be thrown if this method is called without any subscribers to <see cref="OnBandwidthUpdated"/>.
        /// This is to ensure that object bandwidth is computed and stored only if required.
        /// </exception>
        float GetBandwidthBytes(
            ObjectId objectId,
            BandwidthTypes bandwidthTypes = BandwidthTypes.All,
            NetworkDirection networkDirection = NetworkDirection.SentAndReceived);

        /// <summary>
        /// Event that is triggered when new object bandwidth data is available.
        /// If there are no subscribers, then bandwidth may not be computed and stored
        /// by the underlying implementation.
        /// </summary>
        event Action OnBandwidthUpdated;
        
        /// <summary>
        /// Returns true if the bandwidth cache is empty, which might happen short after starting recording metrics.
        /// </summary>
        bool IsCacheEmpty { get; }
    }
}
