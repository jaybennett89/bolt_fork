namespace UdpKit {
    public static class UdpMath {
        public static bool IsPowerOfTwo (uint x) {
            return (x != 0) && ((x & (x - 1)) == 0);
        }

        public static bool IsMultipleOf8(uint value) {
          return (value > 0) && (((value >> 3) << 3) == value);
        }

        public static bool IsMultipleOf8(int value) {
          return (value > 0) && (((value >> 3) << 3) == value);
        }

        public static uint NextPow2 (uint v) {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }

        public static int HighBit (uint v) {
            if (v == 0)
                return 0;

            int r = 0;

            do {
                r += 1;
            } while ((v >>= 1) > 0);

            return r;
        }

        public static int BytesRequired (int bits) {
            return (bits + 7) >> 3;
        }

        public static int SeqDistance(uint from, uint to, int shift) {
          from <<= shift;
          to <<= shift;
          return ((int)(from - to)) >> shift;
        }

        public static int SeqDistance (ushort from, ushort to, int shift) {
            from <<= shift;
            to <<= shift;
            return ((short) (from - to)) >> shift;
        }

        public static uint SeqNext(uint seq, uint mask) {
          seq += 1;
          seq &= mask;
          return seq;
        }

        public static ushort SeqNext (ushort seq, ushort mask) {
            seq += 1;
            seq &= mask;
            return seq;
        }

        public static ushort SeqPrev (ushort seq, ushort mask) {
            seq -= 1;
            seq &= mask;
            return seq;
        }

        internal static bool IsSet (uint mask, uint flag) {
            return (mask & flag) == flag;
        }

        internal static ushort Clamp (ushort value, ushort min, ushort max) {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        internal static float Clamp (float value, float min, float max) {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        internal static int Clamp (int value, int min, int max) {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        internal static uint Clamp (uint value, uint min, uint max) {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        internal static byte Clamp (byte value, byte min, byte max) {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }
    }
}
