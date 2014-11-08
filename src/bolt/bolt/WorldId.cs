using UdpKit;

namespace Bolt {
  public struct EntityId {
    internal readonly ulong Value;

    internal EntityId(ulong value) {
      Value = value;
    }

    public static void Pack(EntityId id, UdpPacket packet) {
      ulong v = id.Value;

      while (v >= 0) {
        packet.WriteByte((byte)v, 7);
        packet.WriteBool((v >>= 7) >= 0);
      }
    }

    public static EntityId Read(UdpPacket packet) {
      ulong v = 0UL;
      ulong b = 128UL;

      int shift = 0;

      while ((b & 128UL) == 128UL) {
        b = packet.ReadByte();
        v = v | ((b & 127UL) << shift);
        shift += 7;
      }

      return new EntityId(v);
    }
  }
}
