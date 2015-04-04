﻿using System;
using System.Text;

namespace UdpKit {
  public class UdpPacket : IDisposable {
    internal bool IsPooled = true;
    internal UdpPacketPool Pool;

    public bool Write;
    public int Ptr;
    public int Length;
    public byte[] Data;

    /// <summary>
    /// A user-assignable object
    /// </summary>
    public object UserToken {
      get;
      set;
    }

    public int Size {
      get { return Length; }
      set { Length = UdpMath.Clamp(value, 0, Data.Length << 3); }
    }

    public int Position {
      get { return Ptr; }
      set { Ptr = UdpMath.Clamp(value, 0, Length); }
    }

    public bool Done {
      get { return Ptr == Length; }
    }

    public bool Overflowing {
      get { return Ptr > Length; }
    }

    public byte[] ByteBuffer {
      get { return Data; }
    }

    public UdpPacket() {
    }

    public UdpPacket(byte[] arr)
      : this(arr, arr.Length) {
    }

    public UdpPacket(byte[] arr, int size) {
      Ptr = 0;
      Data = arr;
      Length = size << 3;
    }

    public bool CanWrite() {
      return CanWrite(1);
    }

    public bool CanRead() {
      return CanRead(1);
    }

    public bool CanWrite(int bits) {
      return Ptr + bits <= Length;
    }

    public bool CanRead(int bits) {
      return Ptr + bits <= Length;
    }

    public byte[] DuplicateData() {
      byte[] duplicate = new byte[UdpMath.BytesRequired(Ptr)];
      Array.Copy(Data, 0, duplicate, 0, duplicate.Length);
      return duplicate;
    }

    public bool WriteBool(bool value) {
#if TRACE_RW
            if (UdpLog.IsEnabled(UdpLog.TRACE))
                UdpLog.Trace("Writing bool (1 bit)");
#endif
      InternalWriteByte(value ? (byte)1 : (byte)0, 1);
      return value;
    }

    public bool ReadBool() {
#if TRACE_RW
            if (UdpLog.IsEnabled(UdpLog.TRACE))
                UdpLog.Trace("Reading bool (1 bit)");
#endif
      return InternalReadByte(1) == 1;
    }

    public void WriteByte(byte value, int bits) {
#if TRACE_RW
            if (UdpLog.IsEnabled(UdpLog.TRACE))
                UdpLog.Trace("Writing byte ({0} bits)", bits);
#endif
      InternalWriteByte(value, bits);
    }

    public byte ReadByte(int bits) {
#if TRACE_RW
            if (UdpLog.IsEnabled(UdpLog.TRACE))
                UdpLog.Trace("Reading byte ({0} bits)", bits);
#endif
      return InternalReadByte(bits);
    }

    public void WriteByte(byte value) {
      WriteByte(value, 8);
    }

    public byte ReadByte() {
      return ReadByte(8);
    }

    public void WriteUShort(ushort value, int bits) {
#if TRACE_RW
            if (UdpLog.IsEnabled(UdpLog.TRACE))
                UdpLog.Trace("Writing ushort ({0} bits)", bits);
#endif
      if (bits <= 8) {
        InternalWriteByte((byte)(value & 0xFF), bits);
      }
      else {
        InternalWriteByte((byte)(value & 0xFF), 8);
        InternalWriteByte((byte)(value >> 8), bits - 8);
      }
    }

    public ushort ReadUShort(int bits) {
#if TRACE_RW
            if (UdpLog.IsEnabled(UdpLog.TRACE))
                UdpLog.Trace("Reading ushort ({0} bits)", bits);
#endif
      if (bits <= 8) {
        return InternalReadByte(bits);
      }
      else {
        return (ushort)(InternalReadByte(8) | (InternalReadByte(bits - 8) << 8));
      }
    }

    public void WriteUShort(ushort value) {
      WriteUShort(value, 16);
    }

    public ushort ReadUShort() {
      return ReadUShort(16);
    }

    public void WriteShort(short value, int bits) {
      WriteUShort((ushort)value, bits);
    }

    public short ReadShort(int bits) {
      return (short)ReadUShort(bits);
    }

    public void WriteShort(short value) {
      WriteShort(value, 16);
    }

    public short ReadShort() {
      return ReadShort(16);
    }

    public void Serialize(ref uint value, int bits) {
      if (Write) { WriteUInt(value, bits); } else { value = ReadUInt(bits); }
    }

    public void Serialize(ref int value, int bits) {
      if (Write) { WriteInt(value, bits); } else { value = ReadInt(bits); }
    }

    public void WriteUInt(uint value, int bits) {
#if TRACE_RW
            if (UdpLog.IsEnabled(UdpLog.TRACE))
                UdpLog.Trace("Writing uint ({0} bits)", bits);
#endif
      byte
                a = (byte)(value >> 0),
                b = (byte)(value >> 8),
                c = (byte)(value >> 16),
                d = (byte)(value >> 24);

      switch ((bits + 7) / 8) {
        case 1:
          InternalWriteByte(a, bits);
          break;

        case 2:
          InternalWriteByte(a, 8);
          InternalWriteByte(b, bits - 8);
          break;

        case 3:
          InternalWriteByte(a, 8);
          InternalWriteByte(b, 8);
          InternalWriteByte(c, bits - 16);
          break;

        case 4:
          InternalWriteByte(a, 8);
          InternalWriteByte(b, 8);
          InternalWriteByte(c, 8);
          InternalWriteByte(d, bits - 24);
          break;
      }
    }

    public uint ReadUInt(int bits) {
#if TRACE_RW
            if (UdpLog.IsEnabled(UdpLog.TRACE))
                UdpLog.Trace("Reading uint ({0} bits)", bits);
#endif
      int
                a = 0,
                b = 0,
                c = 0,
                d = 0;

      switch ((bits + 7) / 8) {
        case 1:
          a = InternalReadByte(bits);
          break;

        case 2:
          a = InternalReadByte(8);
          b = InternalReadByte(bits - 8);
          break;

        case 3:
          a = InternalReadByte(8);
          b = InternalReadByte(8);
          c = InternalReadByte(bits - 16);
          break;

        case 4:
          a = InternalReadByte(8);
          b = InternalReadByte(8);
          c = InternalReadByte(8);
          d = InternalReadByte(bits - 24);
          break;
      }

      return (uint)(a | (b << 8) | (c << 16) | (d << 24));
    }

    public void WriteUInt(uint value) {
      WriteUInt(value, 32);
    }

    public uint ReadUInt() {
      return ReadUInt(32);
    }

    public void WriteInt(int value, int bits) {
      WriteUInt((uint)value, bits);
    }

    public int ReadInt(int bits) {
      return (int)ReadUInt(bits);
    }

    public void WriteInt(int value) {
      WriteInt(value, 32);
    }

    public int ReadInt() {
      return ReadInt(32);
    }

    public void WriteULong(ulong value, int bits) {
#if TRACE_RW
            if (UdpLog.IsEnabled(UdpLog.TRACE))
                UdpLog.Trace("Writing ulong ({0} bits)", bits);
#endif
      if (bits <= 32) {
        WriteUInt((uint)(value & 0xFFFFFFFF), bits);
      }
      else {
        WriteUInt((uint)(value), 32);
        WriteUInt((uint)(value >> 32), bits - 32);
      }
    }

    public ulong ReadULong(int bits) {
#if TRACE_RW
            if (UdpLog.IsEnabled(UdpLog.TRACE))
                UdpLog.Trace("Reading ulong ({0} bits)", bits);
#endif
      if (bits <= 32) {
        return ReadUInt(bits);
      }
      else {
        ulong a = ReadUInt(32);
        ulong b = ReadUInt(bits - 32);
        return a | (b << 32);
      }
    }

    public void WriteULong(ulong value) {
      WriteULong(value, 64);
    }

    public ulong ReadULong() {
      return ReadULong(64);
    }

    public void WriteLong(long value, int bits) {
      WriteULong((ulong)value, bits);
    }

    public long ReadLong(int bits) {
      return (long)ReadULong(bits);
    }

    public void WriteLong(long value) {
      WriteLong(value, 64);
    }

    public long ReadLong() {
      return ReadLong(64);
    }

    public void WriteFloat(float value) {
#if TRACE_RW
            if (UdpLog.IsEnabled(UdpLog.TRACE))
                UdpLog.Trace("Writing float (32 bits)");
#endif
      UdpByteConverter bytes = value;
      InternalWriteByte(bytes.Byte0, 8);
      InternalWriteByte(bytes.Byte1, 8);
      InternalWriteByte(bytes.Byte2, 8);
      InternalWriteByte(bytes.Byte3, 8);
    }

    public float ReadFloat() {
#if TRACE_RW
            if (UdpLog.IsEnabled(UdpLog.TRACE))
                UdpLog.Trace("Reading float (32 bits)");
#endif
      UdpByteConverter bytes = default(UdpByteConverter);
      bytes.Byte0 = InternalReadByte(8);
      bytes.Byte1 = InternalReadByte(8);
      bytes.Byte2 = InternalReadByte(8);
      bytes.Byte3 = InternalReadByte(8);
      return bytes.Float32;
    }

    public void WriteDouble(double value) {
#if TRACE_RW
            if (UdpLog.IsEnabled(UdpLog.TRACE))
                UdpLog.Trace("Writing double (64 bits)");
#endif
      UdpByteConverter bytes = value;
      InternalWriteByte(bytes.Byte0, 8);
      InternalWriteByte(bytes.Byte1, 8);
      InternalWriteByte(bytes.Byte2, 8);
      InternalWriteByte(bytes.Byte3, 8);
      InternalWriteByte(bytes.Byte4, 8);
      InternalWriteByte(bytes.Byte5, 8);
      InternalWriteByte(bytes.Byte6, 8);
      InternalWriteByte(bytes.Byte7, 8);
    }

    public double ReadDouble() {
#if TRACE_RW
            if (UdpLog.IsEnabled(UdpLog.TRACE))
                UdpLog.Trace("Reading double (64 bits)");
#endif
      UdpByteConverter bytes = default(UdpByteConverter);
      bytes.Byte0 = InternalReadByte(8);
      bytes.Byte1 = InternalReadByte(8);
      bytes.Byte2 = InternalReadByte(8);
      bytes.Byte3 = InternalReadByte(8);
      bytes.Byte4 = InternalReadByte(8);
      bytes.Byte5 = InternalReadByte(8);
      bytes.Byte6 = InternalReadByte(8);
      bytes.Byte7 = InternalReadByte(8);
      return bytes.Float64;
    }

    public void WriteByteArray(byte[] from) {
      WriteByteArray(from, 0, from.Length);
    }

    public void WriteByteArray(byte[] from, int count) {
      WriteByteArray(from, 0, count);
    }

    public void WriteByteArray(byte[] from, int offset, int count) {
#if TRACE_RW
            if (UdpLog.IsEnabled(UdpLog.TRACE))
                UdpLog.Trace("Writing byte array ({0} bits)", count * 8);
#endif
      int p = Ptr >> 3;
      int bitsUsed = Ptr % 8;
      int bitsFree = 8 - bitsUsed;

      if (bitsUsed == 0) {
        Buffer.BlockCopy(from, offset, Data, p, count);
      }
      else {
        for (int i = 0; i < count; ++i) {
          byte value = from[offset + i];

          Data[p] &= (byte)(0xFF >> bitsFree);
          Data[p] |= (byte)(value << bitsUsed);

          p += 1;

          Data[p] &= (byte)(0xFF << bitsUsed);
          Data[p] |= (byte)(value >> bitsFree);
        }
      }

      Ptr += (count * 8);
    }

    public byte[] ReadByteArray(int size) {
      byte[] data = new byte[size];
      ReadByteArray(data);
      return data;
    }

    public void ReadByteArray(byte[] to) {
      ReadByteArray(to, 0, to.Length);
    }

    public void ReadByteArray(byte[] to, int count) {
      ReadByteArray(to, 0, count);
    }

    public void ReadByteArray(byte[] to, int offset, int count) {
#if TRACE_RW
            if (UdpLog.IsEnabled(UdpLog.TRACE))
                UdpLog.Trace("Reading byte array ({0} bits)", count * 8);
#endif

      int p = Ptr >> 3;
      int bitsUsed = Ptr % 8;

      if (bitsUsed == 0) {
        Buffer.BlockCopy(Data, p, to, offset, count);
      }
      else {
        int bitsNotUsed = 8 - bitsUsed;

        for (int i = 0; i < count; ++i) {
          int first = Data[p] >> bitsUsed;

          p += 1;

          int second = Data[p] & (255 >> bitsNotUsed);
          to[offset + i] = (byte)(first | (second << bitsNotUsed));
        }
      }

      Ptr += (count * 8);
    }

    public void WriteByteArrayWithPrefix(byte[] array) {
      WriteByteArrayLengthPrefixed(array, 1 << 16);
    }

    public void WriteByteArrayLengthPrefixed(byte[] array, int maxLength) {
      if (WriteBool(array != null)) {
        int length = Math.Min(array.Length, maxLength);

        if (length < array.Length) {
          UdpLog.Warn("Only sendig {0}/{1} bytes from byte array", length, array.Length);
        }

        WriteUShort((ushort)length);
        WriteByteArray(array, 0, length);
      }
    }

    public byte[] ReadByteArrayWithPrefix() {
      if (ReadBool()) {
        int length = ReadUShort();
        byte[] data = new byte[length];

        ReadByteArray(data, 0, data.Length);

        return data;
      }
      else {
        return null;
      }
    }

    public void WriteString(string value, Encoding encoding) {
      WriteString(value, encoding, int.MaxValue);
    }

    public void WriteString(string value, Encoding encoding, int length) {
      if (string.IsNullOrEmpty(value)) {
        WriteUShort(0);
      }
      else {
        if (length < value.Length) {
          value = value.Substring(0, length);
        }

        byte[] bytes = encoding.GetBytes(value);
        WriteUShort((ushort)bytes.Length);
        WriteByteArray(bytes);
      }
    }

    public void WriteString(string value) {
      WriteString(value, Encoding.UTF8);
    }

    public string ReadString(Encoding encoding) {
      int byteCount = ReadUShort();

      if (byteCount == 0) {
        return "";
      }

      var bytes = new byte[byteCount];

      ReadByteArray(bytes);

      return encoding.GetString(bytes, 0, bytes.Length);
    }

    public string ReadString() {
      return ReadString(Encoding.UTF8);
    }

    public void WriteGuid(Guid guid) {
      WriteByteArray(guid.ToByteArray());
    }

    public Guid ReadGuid() {
      byte[] bytes = new byte[16];
      ReadByteArray(bytes);
      return new Guid(bytes);
    }

    public void WriteEndPoint(UdpEndPoint endpoint) {
      WriteByte(endpoint.Address.Byte3);
      WriteByte(endpoint.Address.Byte2);
      WriteByte(endpoint.Address.Byte1);
      WriteByte(endpoint.Address.Byte0);
      WriteUShort(endpoint.Port);
    }

    public UdpEndPoint ReadEndPoint() {

      byte a, b, c, d;

      a = ReadByte();
      b = ReadByte();
      c = ReadByte();
      d = ReadByte();

      ushort port = ReadUShort();

      UdpIPv4Address address = new UdpIPv4Address(a, b, c, d);
      return new UdpEndPoint(address, port);
    }

    void InternalWriteByte(byte value, int bits) {
      WriteByteAt(Data, Ptr, bits, value);

      Ptr += bits;
    }

    public static void WriteByteAt(byte[] data, int ptr, int bits, byte value) {
      if (bits <= 0)
        return;

      value = (byte)(value & (0xFF >> (8 - bits)));

      int p = ptr >> 3;
      int bitsUsed = ptr & 0x7;
      int bitsFree = 8 - bitsUsed;
      int bitsLeft = bitsFree - bits;

      if (bitsLeft >= 0) {
        int mask = (0xFF >> bitsFree) | (0xFF << (8 - bitsLeft));
        data[p] = (byte)((data[p] & mask) | (value << bitsUsed));
      }
      else {
        data[p] = (byte)((data[p] & (0xFF >> bitsFree)) | (value << bitsUsed));
        data[p + 1] = (byte)((data[p + 1] & (0xFF << (bits - bitsFree))) | (value >> bitsFree));
      }
    }

    byte InternalReadByte(int bits) {
      if (bits <= 0)
        return 0;

      byte value;
      int p = Ptr >> 3;
      int bitsUsed = Ptr % 8;

      if (bitsUsed == 0 && bits == 8) {
        value = Data[p];
      }
      else {
        int first = Data[p] >> bitsUsed;
        int remainingBits = bits - (8 - bitsUsed);

        if (remainingBits < 1) {
          value = (byte)(first & (0xFF >> (8 - bits)));
        }
        else {
          int second = Data[p + 1] & (0xFF >> (8 - remainingBits));
          value = (byte)(first | (second << (bits - remainingBits)));
        }
      }

      Ptr += bits;
      return value;
    }

    public void Dispose() {
      if (Pool != null) {
        Pool.Release(this);
      }
    }
  }
}