using System.Diagnostics;

namespace UdpKit {
    public class UdpStats {
        public uint BytesSent { get; private set; }
        public uint BytesReceived { get; private set; }

        public uint PacketsSent { get; private set; }
        public uint PacketsLost { get; private set; }
        public uint PacketsReceived { get; private set; }
        public uint PacketsDelivered { get; private set; }

#if !RELEASE_STATS
        [Conditional("DEBUG")]
#endif
        internal void PacketReceived (uint size) {
            PacketsReceived += 1;
            BytesReceived += size;
        }

#if !RELEASE_STATS
        [Conditional("DEBUG")]
#endif
        internal void PacketSent (uint size) {
            PacketsSent += 1;
            BytesSent += size;
        }

#if !RELEASE_STATS
        [Conditional("DEBUG")]
#endif
        internal void PacketLost () {
            PacketsLost += 1;
        }

#if !RELEASE_STATS
        [Conditional("DEBUG")]
#endif
        internal void PacketDelivered () {
            PacketsDelivered += 1;
        }
    }
}
