namespace UdpKit {
    struct UdpHeader {
        public const int SEQ_BITS = 15;
        public const int SEQ_PADD = 16 - SEQ_BITS;
        public const int SEQ_MASK = (1 << SEQ_BITS) - 1;
        public const int NETPING_BITS = 16;

        public ushort ObjSequence;
        public ushort AckSequence;
        public ulong AckHistory;
        public ushort AckTime;
        public bool IsObject;
        public uint Now;

        public void Pack (UdpStream buffer, UdpSocket socket) {
            int pos = buffer.Position;

            buffer.Position = 0;
            buffer.WriteUShort(PadSequence(ObjSequence), SEQ_BITS + SEQ_PADD);
            buffer.WriteUShort(PadSequence(AckSequence), SEQ_BITS + SEQ_PADD);
            buffer.WriteULong(AckHistory, UdpSocket.AckRedundancy);

            if (UdpSocket.CalculateNetworkPing) {
                buffer.WriteUShort(AckTime, NETPING_BITS);
            }

            buffer.Position = pos;
        }

        public void Unpack (UdpStream buffer, UdpSocket socket) {
            buffer.Position = 0;

            ObjSequence = TrimSequence(buffer.ReadUShort(SEQ_BITS + SEQ_PADD));
            AckSequence = TrimSequence(buffer.ReadUShort(SEQ_BITS + SEQ_PADD));
            AckHistory = buffer.ReadULong(UdpSocket.AckRedundancy);

            if (UdpSocket.CalculateNetworkPing) {
                AckTime = buffer.ReadUShort(NETPING_BITS);
            }
        }

        ushort PadSequence (ushort sequence) {
            sequence <<= SEQ_PADD;

            if (IsObject)
                sequence |= ((1 << SEQ_PADD) - 1);

            return sequence;
        }

        ushort TrimSequence (ushort sequence) {
            sequence >>= SEQ_PADD;
            return sequence;
        }
    }
}
