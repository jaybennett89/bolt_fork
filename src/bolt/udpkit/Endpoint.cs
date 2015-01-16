using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UdpKit {
  public struct UdpSteamID {
    public readonly ulong Id;

    public UdpSteamID(ulong id) {
      Id = id;
    }
  }

  [StructLayout(LayoutKind.Explicit, Pack = 1)]
  public struct UdpEndPoint : IEquatable<UdpEndPoint>, IComparable<UdpEndPoint> {
    public class Comparer : IEqualityComparer<UdpEndPoint> {
      bool IEqualityComparer<UdpEndPoint>.Equals(UdpEndPoint x, UdpEndPoint y) {
        return UdpEndPoint.Compare(x, y) == 0;
      }

      int IEqualityComparer<UdpEndPoint>.GetHashCode(UdpEndPoint obj) {
        return obj.GetHashCode();
      }
    }

    public static readonly UdpEndPoint Any = new UdpEndPoint(UdpIPv4Address.Any, 0);

    [FieldOffset(0)]
    public readonly ushort Port;

    [FieldOffset(4)]
    public readonly UdpIPv4Address Address;

    [FieldOffset(0)]
    public readonly UdpSteamID SteamId;

    public bool IsWan {
      get { return Address.IsWan && Port > 0; }
    }

    public bool IsLan {
      get { return Address.IsPrivate && Port > 0; }
    }

    public UdpEndPoint(UdpIPv4Address address, ushort port) {
      this.SteamId = new UdpSteamID();
      this.Address = address;
      this.Port = port;
    }

    public UdpEndPoint(UdpSteamID steamId) {
      this.Port = 0;
      this.Address = new UdpIPv4Address();
      this.SteamId = steamId;
    }

    public int CompareTo(UdpEndPoint other) {
      return Compare(this, other);
    }

    public bool Equals(UdpEndPoint other) {
      return Compare(this, other) == 0;
    }

    public override int GetHashCode() {
      return (int)(Address.Packed ^ Port);
    }

    public override bool Equals(object obj) {
      if (obj is UdpEndPoint) {
        return Compare(this, (UdpEndPoint)obj) == 0;
      }

      return false;
    }

    public override string ToString() {
      return string.Format("[EndPoint {0}.{1}.{2}.{3}:{4} | {5}]", Address.Byte3, Address.Byte2, Address.Byte1, Address.Byte0, Port, SteamId.Id);
    }

    public static UdpEndPoint Parse(string endpoint) {
      string[] parts = endpoint.Split(':');

      if (parts.Length != 2) {
        throw new FormatException("endpoint is not in the correct format");
      }

      UdpIPv4Address address = UdpIPv4Address.Parse(parts[0]);
      return new UdpEndPoint(address, ushort.Parse(parts[1]));
    }

    public static bool operator ==(UdpEndPoint x, UdpEndPoint y) {
      return Compare(x, y) == 0;
    }

    public static bool operator !=(UdpEndPoint x, UdpEndPoint y) {
      return Compare(x, y) != 0;
    }

    public static UdpEndPoint operator &(UdpEndPoint a, UdpEndPoint b) {
      return new UdpEndPoint(a.Address & b.Address, (ushort)(a.Port & b.Port));
    }

    static int Compare(UdpEndPoint x, UdpEndPoint y) {
      int cmp = x.Address.CompareTo(y.Address);

      if (cmp == 0) {
        cmp = x.Port.CompareTo(y.Port);
      }

      return cmp;
    }

  }
}
