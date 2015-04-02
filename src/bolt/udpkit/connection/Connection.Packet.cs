using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit {
  partial class UdpConnection {
    internal void OnPacketSend(UdpPacket packet) {
      if (IsConnected == false) {
        Socket.Raise(UdpEvent.PUBLIC_PACKET_LOST, this, packet);
        return;
      }

      byte[] buffer;

      buffer = Socket.GetSendBuffer();
      Blit.PackBytes(buffer, PacketPipe.Config.HeaderSize, packet.Data, UdpMath.BytesRequired(packet.Position));

      if (PacketPipe.WriteHeader(buffer, packet)) {
        Socket.Send(EndPoint, buffer, PacketPipe.Config.HeaderSize + UdpMath.BytesRequired(packet.Position));
      }
      else {
        // 
        Socket.Raise(UdpEvent.PUBLIC_PACKET_LOST, this, packet);

        // disconnect with this error
        ConnectionError(UdpConnectionError.SendWindowFull);
      }
    }

    internal void OnPacketReceived(byte[] buffer, int bytes) {
      RecvTime = Socket.GetCurrentTime();

      if (CheckState(UdpConnectionState.Connected) == false) {
        return;
      }

      if (PacketPipe.ReadHeader(buffer, bytes)) {
        UdpPacket packet;

        // create stream and set total size
        packet = Socket.PacketPool.Acquire();
        packet.Size = (bytes - PacketPipe.Config.HeaderSize) << 3;

        // copy data into stream
        Blit.ReadBytes(buffer, PacketPipe.Config.HeaderSize, packet.Data, bytes - PacketPipe.Config.HeaderSize);

        // send to user thread
        Socket.Raise(UdpEvent.PUBLIC_PACKET_RECEIVED, this, packet);
      }
    }
  }
}
