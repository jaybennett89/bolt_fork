using System.Collections.Generic;

namespace UdpKit {
  public class UdpPacketPool {
    readonly UdpSocket socket;
    readonly Stack<UdpPacket> pool = new Stack<UdpPacket>();

    internal UdpPacketPool (UdpSocket s) {
      socket = s;
    }

    internal void Release (UdpPacket stream) {
      UdpAssert.Assert(stream.IsPooled == false);

      lock (pool) {
        stream.Size = 0;
        stream.Position = 0;
        stream.IsPooled = true;

        pool.Push(stream);
      }
    }

    public UdpPacket Acquire () {
      UdpPacket stream = null;

      lock (pool) {
        if (pool.Count > 0) {
          stream = pool.Pop();
        }
      }

      if (stream == null) {
        stream = new UdpPacket(new byte[socket.Config.PacketDatagramSize * 2]);
        stream.Pool = this;
      }

      UdpAssert.Assert(stream.IsPooled);

      stream.IsPooled = false;
      stream.Position = 0;
      stream.Size = (socket.Config.PacketDatagramSize - socket.PacketPipeConfig.HeaderSize) << 3;

      return stream;
    }

    public void Free () {
      lock (pool) {
        while (pool.Count > 0) {
          pool.Pop();
        }
      }
    }
  }
}