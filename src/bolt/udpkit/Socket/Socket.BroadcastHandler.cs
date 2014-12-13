using System;

namespace UdpKit {
  partial class UdpSocket {
    void ProcessLanBroadcastDiscovery() {
      broadcastHandler.Update(GetCurrentTime());
      sessionHandler.RemoveOldSessions(GetCurrentTime());
    }

    class BroadcastHandler {
      uint broadcastTime;
      bool broadcastSession;

      UdpSocket udpSocket;
      UdpPlatformSocket broadcastSocket;
      UdpEndPoint broadcastEndPoint;

      public BroadcastHandler(UdpSocket s) {
        udpSocket = s;
        broadcastTime = 0;
      }

      void RecvData(uint now) {

        while (true) {
#if !DEBUG
          try 
#endif
          {
            if (broadcastSocket.RecvPoll(0)) {
              var bytes = 0;
              var buffer = udpSocket.GetRecvBuffer();
              var remote = new UdpEndPoint();

              bytes = broadcastSocket.RecvFrom(buffer, ref remote);

              if (bytes > 0) {
                var o = 0;
                var guid = Blit.ReadGuid(buffer, ref o);
                var port = Blit.ReadU16(buffer, ref o);
                var name = Blit.ReadString(buffer, ref o);
                var data = Blit.ReadBytesPrefix(buffer, ref o);

                if (guid != udpSocket.sessionHandler.Id) {
                  UdpSession session = new UdpSession();
                  session.Id = guid;
                  session.LastSeen = broadcastSocket.Platform.GetPrecisionTime();
                  session.LanEndPoint = new UdpEndPoint(remote.Address, port);
                  session.HostName = name;
                  session.HostData = data;

                  udpSocket.sessionHandler.UpdateSession(session);
                }
              }
              else {
                return;
              }
            }
            else {
              return;
            }
          }

#if !DEBUG
          catch (Exception exn) {
            UdpLog.Error(exn.ToString());
          }
#endif
        }
      }

      internal void SendData(uint now) {
        if (broadcastSession == false) {
          return;
        }

        if ((broadcastTime + 2000) > now) {
          return;
        }

#if !DEBUG
        try 
#endif
        {
          var buffer = udpSocket.GetSendBuffer();
          var o = 0;

          Blit.PackGuid(buffer, ref o, udpSocket.sessionHandler.Id);
          Blit.PackU16(buffer, ref o, udpSocket.LocalEndPoint.Port);
          Blit.PackString(buffer, ref o, udpSocket.sessionHandler.Name);
          Blit.PackBytesPrefix(buffer, ref o, udpSocket.sessionHandler.Data);

          broadcastSocket.SendTo(buffer, o, broadcastEndPoint);
        }

#if !DEBUG
        catch (Exception exn) {
          UdpLog.Error(exn.ToString());
        }
        finally 
#endif

        {
          broadcastTime = now;
        }
      }

      internal void Enable(UdpEndPoint multicastEndPoint) {
        if (udpSocket.platform.SupportsBroadcast == false) {
          UdpLog.Error("The current plaform {0} does not support LAN broadcast.", udpSocket.platform.GetType());
          return;
        }

        if (broadcastSocket.IsBound) {
          if (broadcastSocket.EndPoint.Port == multicastEndPoint.Port) {
            return;
          }

          broadcastSocket.Close();
          broadcastSocket = null;
        }

        broadcastEndPoint = multicastEndPoint;
        broadcastSocket = udpSocket.platform.CreateSocket();
        broadcastSocket.Bind(new UdpEndPoint(UdpIPv4Address.Any, multicastEndPoint.Port));

        UdpLog.Info("LAN broadcast enabled");
      }

      internal void Disable() {
        if (broadcastSocket != null) {
          if (broadcastSocket.IsBound) {
            broadcastSocket.Close();
          }

          broadcastSocket = null;
          broadcastEndPoint = UdpEndPoint.Any;

          UdpLog.Info("LAN broadcast disabled");
        }
      }

      internal void Update(uint now) {
        if (udpSocket.platform.SupportsBroadcast && (broadcastSocket != null) && broadcastSocket.IsBound) {
          RecvData(now);
          SendData(now);
        }
      }
    }

  }
}
