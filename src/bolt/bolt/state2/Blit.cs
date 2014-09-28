using System;
using System.Runtime.InteropServices;

namespace Bolt {
  public static class Blit {
    public static bool Diff(byte[] a, byte[] b, int offset, int length) {
      Assert.True(a != null);
      Assert.True(b != null);
      Assert.True(a.Length == b.Length);

      int count = a.Length;

      while (length > 0) {
        if (a[offset] != b[offset]) {
          return true;
        }

        ++offset;
        --length;
      }

      return false;
    }

    public static void PackBool(this byte[] data, int offset, bool value) {
      data[offset] = (value ? (byte)1 : (byte)0);
    }

    public static bool ReadBool(this byte[] data, int offset) {
      return data[offset] == 1;
    }

    public static void PackI32(this byte[] data, int offset, int value) {
      data[offset + 0] = (byte)value;
      data[offset + 1] = (byte)(value >> 8);
      data[offset + 2] = (byte)(value >> 16);
      data[offset + 3] = (byte)(value >> 24);
    }

    public static int ReadI32(this byte[] data, int offset) {
      return (data[offset + 0]) | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24);
    }

    public static void PackU32(this byte[] data, uint offset, int value) {
      data[offset + 0] = (byte)value;
      data[offset + 1] = (byte)(value >> 8);
      data[offset + 2] = (byte)(value >> 16);
      data[offset + 3] = (byte)(value >> 24);
    }

    public static uint ReadU32(this byte[] data, int offset) {
      return (uint)((data[offset + 0]) | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24));
    }

    public static void PackF32(this byte[] data, int offset, float value) {
      BitUnion c = default(BitUnion);
      c.Float32 = value;
      data[offset + 0] = c.Byte0;
      data[offset + 1] = c.Byte1;
      data[offset + 2] = c.Byte2;
      data[offset + 3] = c.Byte3;
    }

    public static float ReadF32(this byte[] data, int offset) {
      BitUnion c = default(BitUnion);
      c.Byte0 = data[offset + 0];
      c.Byte1 = data[offset + 1];
      c.Byte2 = data[offset + 2];
      c.Byte3 = data[offset + 3];
      return c.Float32;
    }

    public static void PackBytes(byte[] data, int offset, byte[] bytes) {
      Array.Copy(bytes, 0, data, offset, bytes.Length);
    }

    [StructLayout(LayoutKind.Explicit)]
    struct BitUnion {
      [FieldOffset(0)]
      public Single Float32;

      [FieldOffset(0)]
      public Byte Byte0;
      [FieldOffset(1)]
      public Byte Byte1;
      [FieldOffset(2)]
      public Byte Byte2;
      [FieldOffset(3)]
      public Byte Byte3;
    }

  }
}