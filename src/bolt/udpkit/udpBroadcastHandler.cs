using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit {
  partial class UdpSocket {
    class UdpBroadcastHandler {
      uint broadcastTime;
      bool broadcastSession;
      UdpSocket socket;

      public UdpBroadcastHandler (UdpSocket s) {
        socket = s;
        broadcastTime = 0;
      }

      internal void ReadData (uint now) {
        if (broadcastSession) {
          return;
        }

        int bytes;
        UdpEndPoint remote;
        UdpStream stream = socket.GetReadStream();

        while (socket.platform.RecvBroadcastData(stream.ByteBuffer, out remote, out bytes)) {
          Guid guid = stream.ReadGuid();
          ushort port = stream.ReadUShort();
          string name = stream.ReadString(System.Text.Encoding.UTF8);
          string data = stream.ReadString(System.Text.Encoding.UTF8);

          if (guid != socket.sessionHandler.Id) {
            UdpSession session = new UdpSession();
            session.LastUpdate = now;
            session.EndPoint = new UdpEndPoint(remote.Address, port);
            session.ServerName = name;
            session.UserData = data;
            session.Source = "LAN";

            socket.sessionHandler.UpdateSession(session);

            // reset stream
            stream = socket.GetReadStream();
          }
        }
      }

      internal void PackData (uint now) {
        if (broadcastSession == false) {
          return;
        }

        if ((broadcastTime + 2000) > now) {
          return;
        }

        try {
          UdpStream write = socket.GetWriteStream();
          write.WriteGuid(socket.sessionHandler.Id);
          write.WriteUShort(socket.LocalPhysicalEndPoint.Port);
          write.WriteString(socket.sessionHandler.Name, System.Text.Encoding.UTF8);
          write.WriteString(socket.sessionHandler.Data, System.Text.Encoding.UTF8);

          // send data to everyone
          socket.platform.SendBroadcastData(write.ByteBuffer, UdpMath.BytesRequired(write.Position));
        } finally {
          broadcastTime = now;
        }
      }

      internal void Enable (UdpEndPoint ep, bool shouldBroadcast) {
        if (socket.platform.SupportsBroadcast == false) {
          UdpLog.Error("The current plaform {0} does not support broadcasting.", socket.platform.GetType());
          return;
        }

        broadcastSession = shouldBroadcast;
        socket.platform.EnableBroadcast(ep);
        UdpLog.Info("lan broadcast enabled");
      }

      internal void Disable () {
        socket.sessionHandler.Sessions.Filter(x => x.Source != "LAN");
        socket.platform.DisableBroadcast();
        UdpLog.Info("lan broadcast disabled");
      }
    }
  }
}
