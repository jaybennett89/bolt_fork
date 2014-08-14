using System;
using System.Collections.Generic;
using UdpKit;

static class BoltPacketPool {
  static readonly object sync = new object();
  static readonly Stack<BoltPacket> pool = new Stack<BoltPacket>(1024);

  public static BoltPacket Acquire () {
    BoltPacket packet = null;

    lock (sync) {
      if (pool.Count > 0) {
        packet = pool.Pop();
      }
    }

    if (packet == null) {
      packet = new BoltPacket();
      packet.stream = new UdpStream(new byte[BoltCore.udpConfig.PacketSize * 2]);
    }

    Assert.True(packet.pooled);

    packet.stream.Position = 0;
    packet.stream.Size = BoltPacket.packetSize << 3;
    packet.pooled = false;
    return packet;
  }

  public static void Dispose (BoltPacket packet) {
    Assert.False(packet.pooled);
    Array.Clear(packet.stream.ByteBuffer, 0, packet.stream.ByteBuffer.Length);

    lock (sync) {
      if (packet.userToken != null) {
        packet.userToken.Dispose();
        packet.userToken = null;
      }

      packet.pooled = true;
      pool.Push(packet);
    }
  }
}
