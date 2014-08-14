using System;
using UnityEngine;

static class BoltMath {
    public static uint Bit (int shift) {
        return 1u << shift;
    }

    public static int SequenceDistance (byte from, byte to, int shift) {
        to <<= shift;
        from <<= shift;
        return ((sbyte) (from - to)) >> shift;
    }

    public static int SequenceDistance (uint from, uint to, int shift) {
        from <<= shift;
        to <<= shift;
        return ((int) (from - to)) >> shift;
    }

    public static bool IsSet (byte mask, byte flag) {
        return (mask & flag) == flag;
    }

    public static int PopCount (uint value) {
        int count = 0;

        for (int i = 0; i < 32; ++i) {
            if ((value & (1u << i)) != 0) {
                count += 1;
            }
        }

        return count;
    }

    public static int PopCount (ulong value) {
        int count = 0;

        for (int i = 0; i < 32; ++i) {
            if ((value & (1UL << i)) != 0) {
                count += 1;
            }
        }

        return count;
    }

    public static int Hibit (uint v) {
        int bit = 0;

        while (v > 0) {
            bit += 1;
            v >>= 1;
        }

        return bit;
    }

    public static int BytesRequired (int bits) {
        return (bits + 7) >> 3;
    }
}
