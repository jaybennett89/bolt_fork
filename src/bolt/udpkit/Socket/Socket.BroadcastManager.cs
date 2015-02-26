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
          service.Client.Recv(endpoint, service.Client.Buffer, 0);
        }

        if (socket.Mode == UdpSocketMode.Client) {
          if ((service.SendTime + socket.Config.BroadcastInterval) < now) {
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
        
        // create broadcasting service
        service = new Protocol.ProtocolService(new Protocol.ProtocolClient(socket.platform.CreateBroadcastSocket(new UdpEndPoint(args.LocalAddress, args.Port)), socket.GameId, socket.PeerId));

        // register messages
        service.Client.SetHandler<Protocol.BroadcastSearch>(OnBroadcastSearch);
        service.Client.SetHandler<Protocol.BroadcastSession>(OnBroadcastSession);
      }

      void OnBroadcastSearch(Protocol.BroadcastSearch search) {
        if (search.PeerId != socket.PeerId) {
          var session = service.Client.CreateMessage<Protocol.BroadcastSession>();
          session.Host = socket.sessionManager.GetLocalSession();
          session.Port = socket.platformSocket.EndPoint.Port;

          service.Send(search.Sender, session);
        }
      }

      void OnBroadcastSession(Protocol.BroadcastSession session) {
        if (session.PeerId != socket.PeerId) {
          var addr = session.Sender.Address;
          var port = session.Port;

          // set lan end point of session we received
          session.Host._lanEndPoint = new UdpEndPoint(addr, (ushort)port);

          // update session
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
