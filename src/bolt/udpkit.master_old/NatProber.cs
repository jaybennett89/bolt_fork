using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace UdpKit {
  class NatProber : MasterThread {
    readonly Protocol.Service Probe0;
    readonly Protocol.Service Probe1;
    readonly Protocol.Service Probe2;

    public NatProber(UdpPlatform platform)
      : base(platform) {
      Probe0 = new Protocol.Service();
      Probe0.Peer.Socket = platform.CreateSocket();

      Probe1 = new Protocol.Service();
      Probe1.Peer.Socket = platform.CreateSocket();

      Probe2 = new Protocol.Service();
      Probe2.Peer.Socket = platform.CreateSocket();
    }

    protected override void OnInit() {
      BindSocket(Probe0.Peer.Socket, Config.Probe0);
      BindSocket(Probe1.Peer.Socket, Config.Probe1);
      BindSocket(Probe2.Peer.Socket, Config.Probe2);

      if (Running) {
        Probe0.Peer.SetHandler<Proto.ProbeEndPoint>(MakeOnProbeRequest(Probe0));
        Probe1.Peer.SetHandler<Proto.ProbeEndPoint>(MakeOnProbeRequest(Probe1));
      }
    }

    protected override void OnUpdate() {
      Probe0.Peer.Recv(0);
      Probe1.Peer.Recv(1);
    }

    void BindSocket(UdpPlatformSocket socket, UdpEndPoint endpoint) {
      socket.Bind(endpoint);

      if (socket.IsBound) {
        UdpLog.Info("bound nat-probe socket {0}", endpoint);
      }
      else {
        Shutdown("nat-probe socket failed to bind to {1}: {2}", endpoint, socket.Error);
      }
    }

    Action<Protocol.ProbeEndPoint> MakeOnProbeRequest(Protocol.Service probe) {
      return request => {
        Protocol.ProbeEndPointResult result;
        result = probe.Peer.Create<Proto.ProbeEndPointResult>();
        result.Query = request.MessageId;
        result.WanEndPoint = request.Sender;

        probe.Send(result, result.WanEndPoint);

        if (ReferenceEquals(probe, Probe0)) {
          Protocol.ProbeUnsolicited unsolicited;
          unsolicited = probe.Peer.Create<Proto.ProbeUnsolicited>();
          unsolicited.WanEndPoint = request.Sender;

          Probe2.Send(unsolicited, unsolicited.WanEndPoint);
        }
      };
    }
  }
}
