using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;

namespace UdpKit {
  [StructLayout(LayoutKind.Explicit, Pack = 1)]
  public struct UdpIPv4Address : IEquatable<UdpIPv4Address>, IComparable<UdpIPv4Address> {
    public class Comparer : IComparer<UdpIPv4Address>, IEqualityComparer<UdpIPv4Address> {
      public static readonly Comparer Instance = new Comparer();

      Comparer() {

      }

      int IComparer<UdpIPv4Address>.Compare(UdpIPv4Address x, UdpIPv4Address y) {
        return Compare(x, y);
      }

      bool IEqualityComparer<UdpIPv4Address>.Equals(UdpIPv4Address x, UdpIPv4Address y) {
        return Compare(x, y) == 0;
      }

      int IEqualityComparer<UdpIPv4Address>.GetHashCode(UdpIPv4Address obj) {
        return (int)obj.Packed;
      }
    }

    public static readonly UdpIPv4Address Any = new UdpIPv4Address();
    public static readonly UdpIPv4Address Localhost = new UdpIPv4Address(127, 0, 0, 1);

    [FieldOffset(0)]
    public readonly uint Packed;

    [FieldOffset(0)]
    public byte Byte0;

    [FieldOffset(1)]
    public byte Byte1;

    [FieldOffset(2)]
    public byte Byte2;

    [FieldOffset(3)]
    public byte Byte3;

    public UdpIPv4Address(uint packed) {
      Byte0 = Byte1 = Byte2 = Byte3 = 0;
      Packed = packed;
    }

    public UdpIPv4Address(long addr) {
      Byte0 = Byte1 = Byte2 = Byte3 = 0;
      Packed = (uint)IPAddress.NetworkToHostOrder((int)addr);
    }

    public UdpIPv4Address(byte a, byte b, byte c, byte d) {
      Packed = 0;
      Byte0 = d;
      Byte1 = c;
      Byte2 = b;
      Byte3 = a;
    }

    public bool Equals(UdpIPv4Address other) {
      return Compare(this, other) == 0;
    }

    public int CompareTo(UdpIPv4Address other) {
      return Compare(this, other);
    }

    public override int GetHashCode() {
      return (int)Packed;
    }

    public override bool Equals(object obj) {
      if (obj is UdpIPv4Address) {
        return Compare(this, (UdpIPv4Address)obj) == 0;
      }

      return false;
    }

    public override string ToString() {
      return string.Format("{0}.{1}.{2}.{3}", Byte3, Byte2, Byte1, Byte0);
    }

    public bool IsAny {
      get { return Byte0 == 0 && Byte1 == 0 && Byte2 == 0 && Byte3 == 0; }
    }

    public bool IsLocalHost {
      get { return Byte3 == 127 && Byte2 == 0 && Byte1 == 0 && Byte0 == 1; }
    }

    public bool IsBroadcast {
      get { return Byte3 == 255 && Byte2 == 255 && Byte1 == 255 && Byte0 == 255; }
    }

    public bool IsPrivate {
      get {
        return
          (Byte3 == 10) // 10.*.*.*
          ||
          (Byte3 == 172 && Byte2 == 16) // 172.16.*.*
          ||
          (Byte3 == 192 && Byte2 == 168) // 192.168.*.*
          ;
      }
    }

    public bool IsWan {
      get { return !IsAny && !IsLocalHost && !IsBroadcast && !IsPrivate; }
    }

    public static bool operator ==(UdpIPv4Address x, UdpIPv4Address y) {
      return Compare(x, y) == 0;
    }

    public static bool operator !=(UdpIPv4Address x, UdpIPv4Address y) {
      return Compare(x, y) != 0;
    }

    public static UdpIPv4Address operator &(UdpIPv4Address a, UdpIPv4Address b) {
      return new UdpIPv4Address(a.Packed & b.Packed);
    }

    static int Compare(UdpIPv4Address x, UdpIPv4Address y) {
      if (x.Packed > y.Packed) return 1;
      if (x.Packed < y.Packed) return -1;
      return 0;
    }

    public static UdpIPv4Address Parse(string address) {
      string[] parts = address.Split('.');

      if (parts.Length != 4) {
        throw new FormatException("address is not in the correct format");
      }

      return new UdpIPv4Address(byte.Parse(parts[0]), byte.Parse(parts[1]), byte.Parse(parts[2]), byte.Parse(parts[3]));
    }
  }
}
