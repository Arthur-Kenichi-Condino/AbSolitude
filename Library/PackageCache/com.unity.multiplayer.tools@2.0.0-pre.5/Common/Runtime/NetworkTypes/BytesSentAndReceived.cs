using System;
using System.Collections.Generic;

namespace Unity.Multiplayer.Tools.Common
{
    [Serializable]
    struct BytesSentAndReceived : IEquatable<BytesSentAndReceived>
    {
        public BytesSentAndReceived(long sent = 0, long received = 0)
        {
            Sent = sent;
            Received = received;
        }

        public BytesSentAndReceived(long count, NetworkDirection direction)
        {
            Sent = ((direction & NetworkDirection.Sent) != NetworkDirection.None) ? count : 0;
            Received = ((direction & NetworkDirection.Received) != NetworkDirection.None) ? count : 0;
        }

        public long Sent { get; set; }
        public long Received { get; set; }

        /// <summary>
        /// Indexes this BytesSentAndReceived by direction. This works as follows:
        /// 1. Returns the sent field if the direction is Sent.
        /// 2. Returns the received field if the direction is Received.
        /// 3. Returns the sum if the direction is SentAndReceived.
        /// 4. Returns 0 if the direction is None.
        /// </summary>
        public long this[NetworkDirection direction] =>
            ((direction & NetworkDirection.Sent) != NetworkDirection.None ? Sent : 0) +
            ((direction & NetworkDirection.Received) != NetworkDirection.None ? Received : 0);

        public NetworkDirection Direction =>
            (Sent     > 0f ? NetworkDirection.Sent : NetworkDirection.None) |
            (Received > 0f ? NetworkDirection.Received : NetworkDirection.None);

        public long Total => Sent + Received;

        public bool Equals(BytesSentAndReceived other)
        {
            return Sent == other.Sent &&
                   Received == other.Received;
        }

        public override bool Equals(object obj)
        {
            return obj is BytesSentAndReceived other && Equals(other);
        }

        public static BytesSentAndReceived operator +(
            BytesSentAndReceived a,
            BytesSentAndReceived b) => new BytesSentAndReceived(
                a.Sent + b.Sent,
                a.Received + b.Received);

        public override int GetHashCode() => HashCode.Combine(Sent, Received);

        public override string ToString()
        {
            return $"{nameof(BytesSentAndReceived)}: {nameof(Sent)}={Sent} {nameof(Received)}={Received}";
        }
    }

    internal static class BytesSentAndReceivedExtensions
    {
        public static BytesSentAndReceived Sum<T>(this IEnumerable<T> ts, Func<T, BytesSentAndReceived> f)
        {
            BytesSentAndReceived result = new BytesSentAndReceived();
            foreach (var t in ts)
            {
                result += f(t);
            }
            return result;
        }
    }
}
