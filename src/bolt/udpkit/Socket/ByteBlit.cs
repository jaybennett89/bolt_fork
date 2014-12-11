using System;
using System.Runtime.InteropServices;
using System.Text;

namespace UdpKit {
  public static class Blit {
    public static bool PackBool(byte[] bytes, ref int offset, bool value) {
      PackByte(bytes, ref offset, value ? (byte)1 : (byte)0);
      return value;
    }

    public static bool ReadBool(byte[] bytes, ref int offset) {
      return ReadByte(bytes, ref offset) == 1;
    }

    public static void PackByte(byte[] bytes, ref int offset, byte value) {
      bytes[offset] = value;
      offset += 1;
    }

    public static byte ReadByte(byte[] bytes, ref int offset) {
      byte v = bytes[offset];
      offset += 1;
      return v;
    }

    public static void PackBytesPrefix(byte[] bytes, ref int offset, byte[] from) {
      // pack length prefix
      PackI32(bytes, ref offset, from.Length);

      // copy all data
      Array.Copy(from, 0, bytes, offset, from.Length);

      // increase offset
      offset += from.Length;
    }

    public static byte[] ReadBytesPrefix(byte[] bytes, ref int offset) {
      // create array with size of prefix
      byte[] data = new byte[ReadI32(bytes, ref offset)];

      // copy data
      Array.Copy(bytes, offset, data, 0, data.Length);

      // increment offset
      offset += data.Length;

      return data;
    }

    public static void PackBytes(byte[] bytes, int offset, byte[] from, int length) {
      Array.Copy(from, 0, bytes, offset, length);
    }

    public static void ReadBytes(byte[] bytes, int offset, byte[] into, int length) {
      Array.Copy(bytes, offset, into, 0, length);
    }

    public static void PackBytes(byte[] bytes, ref int bytesOffset, byte[] from, int fromOffset, int length) {
      Array.Copy(from, fromOffset, bytes, bytesOffset, length);
      bytesOffset += length;
    }

    public static void ReadBytes(byte[] bytes, ref int bytesOffset, byte[] into, int intoOffset, int length) {
      Array.Copy(bytes, bytesOffset, into, intoOffset, length);
      bytesOffset += length;
    }

    public static void PackU16(byte[] bytes, ref int offset, ushort value) {
      BitUnion c = default(BitUnion);
      c.UInt16 = value;

      bytes[offset + 0] = c.Byte0;
      bytes[offset + 1] = c.Byte1;

      offset += 2;
    }

    public static ushort ReadU16(byte[] bytes, ref int offset) {
      BitUnion c = default(BitUnion);
      c.Byte0 = bytes[offset + 0];
      c.Byte1 = bytes[offset + 1];

      offset += 2;

      return c.UInt16;
    }

    public static void PackI32(byte[] bytes, ref int offset, int value) {
      BitUnion c;

      c = default(BitUnion);
      c.Int32 = value;

      bytes[offset + 0] = c.Byte0;
      bytes[offset + 1] = c.Byte1;
      bytes[offset + 2] = c.Byte2;
      bytes[offset + 3] = c.Byte3;

      offset += 4;
    }

    public static int ReadI32(byte[] bytes, ref int offset) {
      BitUnion c;

      c = default(BitUnion);
      c.Byte0 = bytes[offset + 0];
      c.Byte1 = bytes[offset + 1];
      c.Byte2 = bytes[offset + 2];
      c.Byte3 = bytes[offset + 3];

      offset += 4;

      return c.Int32;
    }

    public static void PackU32(byte[] bytes, ref int offset, uint value) {
      PackU32(bytes, ref offset, value, 4);
    }

    public static void PackU32(byte[] bytes, ref int offset, uint value, int byteCount) {
      BitUnion c = default(BitUnion);
      c.UInt32 = value;

      switch (byteCount) {
        case 1:
          bytes[offset + 0] = c.Byte0;
          offset += 1;
          break;

        case 2:
          bytes[offset + 0] = c.Byte0;
          bytes[offset + 1] = c.Byte1;
          offset += 2;
          break;

        case 3:
          bytes[offset + 0] = c.Byte0;
          bytes[offset + 1] = c.Byte1;
          bytes[offset + 2] = c.Byte2;
          offset += 3;
          break;

        case 4:
          bytes[offset + 0] = c.Byte0;
          bytes[offset + 1] = c.Byte1;
          bytes[offset + 2] = c.Byte2;
          bytes[offset + 3] = c.Byte3;
          offset += 4;
          break;
      }
    }

    public static uint ReadU32(byte[] bytes, ref int offset) {
      return ReadU32(bytes, ref offset, 4);
    }

    public static uint ReadU32(byte[] bytes, ref int offset, int byteCount) {
      BitUnion c = default(BitUnion);

      switch (byteCount) {
        case 1:
          c.Byte0 = bytes[offset + 0];
          offset += 1;
          break;

        case 2:
          c.Byte0 = bytes[offset + 0];
          c.Byte1 = bytes[offset + 1];
          offset += 2;
          break;

        case 3:
          c.Byte0 = bytes[offset + 0];
          c.Byte1 = bytes[offset + 1];
          c.Byte2 = bytes[offset + 2];
          offset += 3;
          break;

        case 4:
          c.Byte0 = bytes[offset + 0];
          c.Byte1 = bytes[offset + 1];
          c.Byte2 = bytes[offset + 2];
          c.Byte3 = bytes[offset + 3];
          offset += 4;
          break;
      }

      return c.UInt32;
    }

    public static void PackF32(byte[] bytes, int offset, float value) {
      BitUnion c = default(BitUnion);
      c.Float32 = value;
      bytes[offset + 0] = c.Byte0;
      bytes[offset + 1] = c.Byte1;
      bytes[offset + 2] = c.Byte2;
      bytes[offset + 3] = c.Byte3;
    }

    public static float ReadF32(byte[] bytes, int offset) {
      BitUnion c = default(BitUnion);
      c.Byte0 = bytes[offset + 0];
      c.Byte1 = bytes[offset + 1];
      c.Byte2 = bytes[offset + 2];
      c.Byte3 = bytes[offset + 3];
      return c.Float32;
    }

    public static void PackU64(byte[] bytes, ref int offset, UInt64 value) {
      BitUnion64 c;

      c = default(BitUnion64);
      c.UInt64 = value;

      bytes[offset + 0] = c.Byte0;
      bytes[offset + 1] = c.Byte1;
      bytes[offset + 2] = c.Byte2;
      bytes[offset + 3] = c.Byte3;
      bytes[offset + 4] = c.Byte4;
      bytes[offset + 5] = c.Byte5;
      bytes[offset + 6] = c.Byte6;
      bytes[offset + 7] = c.Byte7;

      offset += 8;
    }

    public static UInt64 ReadU64(byte[] bytes, ref int offset) {
      BitUnion64 c;

      c = default(BitUnion64);
      c.Byte0 = bytes[offset + 0];
      c.Byte1 = bytes[offset + 1];
      c.Byte2 = bytes[offset + 2];
      c.Byte3 = bytes[offset + 3];
      c.Byte4 = bytes[offset + 4];
      c.Byte5 = bytes[offset + 5];
      c.Byte6 = bytes[offset + 6];
      c.Byte7 = bytes[offset + 7];

      offset += 8;

      return c.UInt64;
    }

    public static void PackString(byte[] bytes, ref int offset, string value) {
      if (PackBool(bytes, ref offset, value != null)) {
        // store offset for count
        int countOffset = offset;

        // skip 4 bytes for count
        offset += 4;

        // encode bytes into array
        int count = Encoding.UTF8.GetBytes(value, 0, value.Length, bytes, offset);

        // write count into 4 skipped bytes
        PackI32(bytes, ref countOffset, count);

        // done!
        offset += count;
      }
    }

    public static string ReadString(byte[] bytes, ref int offset) {
      if (ReadBool(bytes, ref offset)) {
        var count = ReadI32(bytes, ref offset);
        var value = Encoding.UTF8.GetString(bytes, offset, count);

        offset += count;

        return value; 
      }

      return null;
    }

    public static UdpEndPoint ReadEndPoint(byte[] bytes, ref int offset) {
      BitUnion c;

      UInt32 addr;
      UInt16 port;

      c = default(BitUnion);
      c.Byte0 = ReadByte(bytes, ref offset);
      c.Byte1 = ReadByte(bytes, ref offset);
      c.Byte2 = ReadByte(bytes, ref offset);
      c.Byte3 = ReadByte(bytes, ref offset);

      addr = c.UInt32;

      c = default(BitUnion);
      c.Byte0 = ReadByte(bytes, ref offset);
      c.Byte1 = ReadByte(bytes, ref offset);

      port = c.UInt16;

      return new UdpEndPoint(new UdpIPv4Address(addr), port);
    }

    public static void PackEndPoint(byte[] bytes, ref int offset, UdpEndPoint endpoint) {
      BitUnion c;

      c = default(BitUnion);
      c.UInt32 = endpoint.Address.Packed;
      PackByte(bytes, ref offset, c.Byte0);
      PackByte(bytes, ref offset, c.Byte1);
      PackByte(bytes, ref offset, c.Byte2);
      PackByte(bytes, ref offset, c.Byte3);

      c = default(BitUnion);
      c.UInt16 = endpoint.Port;
      PackByte(bytes, ref offset, c.Byte0);
      PackByte(bytes, ref offset, c.Byte1);

    }

    public static void PackGuid(byte[] bytes, ref int offset, Guid value) {
      BitUnion128 c;

      c = default(BitUnion128);
      c.Guid = value;

      bytes[offset + 0] = c.Byte0;
      bytes[offset + 1] = c.Byte1;
      bytes[offset + 2] = c.Byte2;
      bytes[offset + 3] = c.Byte3;
      bytes[offset + 4] = c.Byte4;
      bytes[offset + 5] = c.Byte5;
      bytes[offset + 6] = c.Byte6;
      bytes[offset + 7] = c.Byte7;
      bytes[offset + 8] = c.Byte8;
      bytes[offset + 9] = c.Byte9;
      bytes[offset + 10] = c.Byte10;
      bytes[offset + 11] = c.Byte11;
      bytes[offset + 12] = c.Byte12;
      bytes[offset + 13] = c.Byte13;
      bytes[offset + 14] = c.Byte14;
      bytes[offset + 15] = c.Byte15;

      offset += 16;
    }

    public static Guid ReadGuid(byte[] bytes, ref int offset) {
      BitUnion128 c;

      c = default(BitUnion128);

      c.Byte0 = bytes[offset + 0];
      c.Byte1 = bytes[offset + 1];
      c.Byte2 = bytes[offset + 2];
      c.Byte3 = bytes[offset + 3];
      c.Byte4 = bytes[offset + 4];
      c.Byte5 = bytes[offset + 5];
      c.Byte6 = bytes[offset + 6];
      c.Byte7 = bytes[offset + 7];
      c.Byte8 = bytes[offset + 8];
      c.Byte9 = bytes[offset + 9];
      c.Byte10 = bytes[offset + 10];
      c.Byte11 = bytes[offset + 11];
      c.Byte12 = bytes[offset + 12];
      c.Byte13 = bytes[offset + 13];
      c.Byte14 = bytes[offset + 14];
      c.Byte15 = bytes[offset + 15];

      offset += 16;

      return c.Guid;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct BitUnion {
      [FieldOffset(0)]
      public UInt16 UInt16;
      [FieldOffset(0)]
      public Int16 Int16;
      [FieldOffset(0)]
      public UInt32 UInt32;
      [FieldOffset(0)]
      public Int32 Int32;
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

    [StructLayout(LayoutKind.Explicit)]
    struct BitUnion64 {
      [FieldOffset(0)]
      public UInt64 UInt64;

      [FieldOffset(0)]
      public Byte Byte0;
      [FieldOffset(1)]
      public Byte Byte1;
      [FieldOffset(2)]
      public Byte Byte2;
      [FieldOffset(3)]
      public Byte Byte3;
      [FieldOffset(4)]
      public Byte Byte4;
      [FieldOffset(5)]
      public Byte Byte5;
      [FieldOffset(6)]
      public Byte Byte6;
      [FieldOffset(7)]
      public Byte Byte7;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct BitUnion128 {
      [FieldOffset(0)]
      public Guid Guid;

      [FieldOffset(0)]
      public Byte Byte0;
      [FieldOffset(1)]
      public Byte Byte1;
      [FieldOffset(2)]
      public Byte Byte2;
      [FieldOffset(3)]
      public Byte Byte3;
      [FieldOffset(4)]
      public Byte Byte4;
      [FieldOffset(5)]
      public Byte Byte5;
      [FieldOffset(6)]
      public Byte Byte6;
      [FieldOffset(7)]
      public Byte Byte7;
      [FieldOffset(8)]
      public Byte Byte8;
      [FieldOffset(9)]
      public Byte Byte9;
      [FieldOffset(10)]
      public Byte Byte10;
      [FieldOffset(11)]
      public Byte Byte11;
      [FieldOffset(12)]
      public Byte Byte12;
      [FieldOffset(13)]
      public Byte Byte13;
      [FieldOffset(14)]
      public Byte Byte14;
      [FieldOffset(15)]
      public Byte Byte15;
    }
  }
}
