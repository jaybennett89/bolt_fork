using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit {
  partial class UdpSocket {
    class BroadcastManager {
      readonly UdpSocket socket;

      Protocol.ProtocolService service;
      UdpEndPoint broadcast;

      public bool IsEnabled {
        get { return service != null && service.Client != null && service.Client.Socket != null && service.Client.Socket.IsBound; }
      }

      public BroadcastManager(UdpSocket s) {
        socket = s;
      }

      public void Update(uint now) {
        if (IsEnabled == false) {
          return;
        }

        if (service.Client.Socket.RecvPoll(0)) {
          var endpoint = new UdpEndPoint();
          var bytes = service.Client.Socket.RecvFrom(service.Client.Buffer, ref endpoint);
          service.Client.Recv(endpoint, service.Client.Buffer, bytes);
        }

        if (socket.Mode == UdpSocketMode.Client) {
          if ((service.SendTime + 2000) < now) {
            service.Send<Protocol.BroadcastSearch>(broadcast);
          }
        }
      }

      public void Enable(UdpEventBroadcastArgs args) {
        if (socket.platform.SupportsBroadcast == false) {
          UdpLog.Error("Current platform: {0}, does not support broadcasting", socket.platform.GetType().Name);
          return;
        }

        if (IsEnabled) {
          service.Client.Socket.Close();
        }

        broadcast = new UdpEndPoint(args.BroadcastAddress, args.Port);

        service = new Protocol.ProtocolService(new Protocol.ProtocolClient(socket.platform.CreateSocket(), socket.GameId, socket.PeerId));
        service.Client.Socket.Bind(new UdpEndPoint(args.LocalAddress, args.Port));
        service.Client.Socket.Broadcast = true;

        service.Client.SetHandler<Protocol.BroadcastSearch>(OnBroadcastSearch);
        service.Client.SetHandler<Protocol.BroadcastSession>(OnBroadcastSession);
      }

      void OnBroadcastSearch(Protocol.BroadcastSearch search) {
        service.Send<Protocol.BroadcastSession>(search.Sender, m => m.Host = socket.sessionManager.GetLocalSession());
      }

      void OnBroadcastSession(Protocol.BroadcastSession session) {
        if (session.Host.Id != socket.PeerId) {
          socket.sessionManager.UpdateSession(session.Host, UdpSessionSource.Lan);
        }
      }

      public void Disable() {
        if (IsEnabled) {
          service.Client.Socket.Close();
        }

        service = null;
        broadcast = new UdpEndPoint();
      }
    }

  }
}
