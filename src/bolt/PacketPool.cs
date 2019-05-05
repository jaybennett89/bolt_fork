using System;
using System.Collections.Generic;
using UdpKit;

namespace Bolt {
  class PacketPool {
    public static Packet Acquire() {
      return new Packet();
    }

    public static void Dispose(Packet packet) {
      packet.UdpPacket.Dispose();
      packet.UdpPacket = null;
    }
  }
}