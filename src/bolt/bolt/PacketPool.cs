using System;
using System.Collections.Generic;
using UdpKit;

namespace Bolt {
  static class PacketPool {
    static readonly object sync = new object();
    static readonly Stack<Packet> pool = new Stack<Packet>(1024);

    public static Packet Acquire() {
      Packet packet = null;

      lock (sync) {
        if (pool.Count > 0) {
          packet = pool.Pop();
        }
      }

      if (packet == null) {
        packet = new Packet();
        packet.UdpPacket = new UdpPacket(new byte[BoltCore._udpConfig.PacketSize * 2]);
      }

      Assert.True(packet.Pooled);

      packet.UdpPacket.Position = 0;
      packet.UdpPacket.Size = Packet.MaxSize << 3;
      packet.Pooled = false;
      return packet;
    }

    public static void Dispose(Packet packet) {
      Assert.False(packet.Pooled);
      Array.Clear(packet.UdpPacket.ByteBuffer, 0, packet.UdpPacket.ByteBuffer.Length);

      lock (sync) {
        packet.Pooled = true;
        pool.Push(packet);
      }
    }
  }
}