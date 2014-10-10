using System;
using System.Runtime.InteropServices;
using UdpKit;

namespace Bolt {
  [StructLayout(LayoutKind.Explicit)]
  public struct UniqueId {
    [FieldOffset(0)]
    Guid guid;

    [FieldOffset(0)]
    uint uint0;

    [FieldOffset(4)]
    uint uint1;

    [FieldOffset(8)]
    uint uint2;

    [FieldOffset(12)]
    uint uint3;

    public string IdString {
      get {
        if (IsNone) {
          return "NONE";
        }

        return guid.ToString();
      }
    }

    public bool IsNone {
      get { return guid == Guid.Empty; }
    }

    public void Pack(UdpStream stream) {
      stream.WriteUInt(uint0);
      stream.WriteUInt(uint1);
      stream.WriteUInt(uint2);
      stream.WriteUInt(uint3);
    }

    public override string ToString() {
      if (IsNone) {
        return "[UniqueId NONE]";
      }

      return string.Format("[UniqueId {0}]", guid.ToString());
    }

    public static UniqueId None {
      get { return default(UniqueId); }
    }

    public static UniqueId New() {
      UniqueId id;

      id = default(UniqueId);
      id.guid = Guid.NewGuid();

      return id;
    }

    public static UniqueId Parse(string text) {
      if (text == "NONE") {
        return None;
      }

      UniqueId id;

      id = default(UniqueId);
      id.guid = new Guid(text);

      return id;
    }

    public static UniqueId Read(UdpStream stream) {
      UniqueId id;

      id = default(UniqueId);
      id.uint0 = stream.ReadUInt();
      id.uint1 = stream.ReadUInt();
      id.uint2 = stream.ReadUInt();
      id.uint3 = stream.ReadUInt();

      return id;
    }

    public static bool operator ==(UniqueId a, UniqueId b) {
      return a.guid == b.guid;
    }

    public static bool operator !=(UniqueId a, UniqueId b) {
      return a.guid != b.guid;
    }
  }

}