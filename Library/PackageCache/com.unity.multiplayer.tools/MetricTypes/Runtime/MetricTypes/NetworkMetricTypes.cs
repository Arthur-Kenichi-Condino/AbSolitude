namespace Unity.Multiplayer.Tools.MetricTypes
{
    /// <summary>
    /// DEPRECATED
    /// <see cref="DirectionalMetricInfo"/> is now just a wrapper around <see cref="DirectedMetricType"/>.
    /// Please use <see cref="DirectedMetricType"/> instead.
    /// <see cref="DirectionalMetricInfo"/> can't be deleted yet as it's still referenced by the NGO library (as of 2021-12-07).
    /// </summary>
    static class NetworkMetricTypes
    {
        public static readonly DirectionalMetricInfo NetworkMessageSent = new(DirectedMetricType.NetworkMessageSent);
        public static readonly DirectionalMetricInfo NetworkMessageReceived = new(DirectedMetricType.NetworkMessageReceived);

        public static readonly DirectionalMetricInfo TotalBytesSent = new(DirectedMetricType.TotalBytesSent);
        public static readonly DirectionalMetricInfo TotalBytesReceived = new(DirectedMetricType.TotalBytesReceived);

        public static readonly DirectionalMetricInfo RpcSent = new(DirectedMetricType.RpcSent);
        public static readonly DirectionalMetricInfo RpcReceived = new(DirectedMetricType.RpcReceived);

        public static readonly DirectionalMetricInfo NamedMessageSent = new (DirectedMetricType.NamedMessageSent);
        public static readonly DirectionalMetricInfo NamedMessageReceived = new (DirectedMetricType.NamedMessageReceived);

        public static readonly DirectionalMetricInfo UnnamedMessageSent = new (DirectedMetricType.UnnamedMessageSent);
        public static readonly DirectionalMetricInfo UnnamedMessageReceived = new (DirectedMetricType.UnnamedMessageReceived);

        public static readonly DirectionalMetricInfo NetworkVariableDeltaSent = new (DirectedMetricType.NetworkVariableDeltaSent);
        public static readonly DirectionalMetricInfo NetworkVariableDeltaReceived = new (DirectedMetricType.NetworkVariableDeltaReceived);

        public static readonly DirectionalMetricInfo ObjectSpawnedSent = new (DirectedMetricType.ObjectSpawnedSent);
        public static readonly DirectionalMetricInfo ObjectSpawnedReceived = new (DirectedMetricType.ObjectSpawnedReceived);

        public static readonly DirectionalMetricInfo ObjectDestroyedSent = new (DirectedMetricType.ObjectDestroyedSent);
        public static readonly DirectionalMetricInfo ObjectDestroyedReceived = new (DirectedMetricType.ObjectDestroyedReceived);

        public static readonly DirectionalMetricInfo OwnershipChangeSent = new (DirectedMetricType.OwnershipChangeSent);
        public static readonly DirectionalMetricInfo OwnershipChangeReceived = new (DirectedMetricType.OwnershipChangeReceived);

        public static readonly DirectionalMetricInfo ServerLogSent = new (DirectedMetricType.ServerLogSent);
        public static readonly DirectionalMetricInfo ServerLogReceived = new (DirectedMetricType.ServerLogReceived);

        public static readonly DirectionalMetricInfo SceneEventSent = new (DirectedMetricType.SceneEventSent);
        public static readonly DirectionalMetricInfo SceneEventReceived = new (DirectedMetricType.SceneEventReceived);

        public static readonly DirectionalMetricInfo PacketsSent = new (DirectedMetricType.PacketsSent);
        public static readonly DirectionalMetricInfo PacketsReceived = new (DirectedMetricType.PacketsReceived);

        public static readonly DirectionalMetricInfo RttToServer = new (DirectedMetricType.RttToServer);

        public static readonly DirectionalMetricInfo NetworkObjects = new (DirectedMetricType.NetworkObjects);

        public static readonly DirectionalMetricInfo ConnectedClients = new (DirectedMetricType.Connections);

        public static readonly DirectionalMetricInfo PacketLoss = new (DirectedMetricType.PacketLoss);
    }
}
