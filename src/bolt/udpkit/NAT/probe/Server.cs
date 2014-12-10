using System;
using System.Collections.Generic;

using System.Text;

namespace UdpKit.NAT.Probe {
  public sealed class Server : UdpKit.NAT.Peer<MasterServer.Config> {
    public const byte PROBE0 = 0;
    public const byte PROBE1 = 1;
    public const byte PROBE2 = 2;
    public const byte PROBE_COUNT = 3;

    readonly Protocol.Peer[] Peers = new Protocol.Peer[PROBE_COUNT];

    public Server(UdpPlatform platform)
      : base(platform) {
    }

    protected override void OnInit() {
      BindSocket(out Peers[0], Config.Probe0);
      BindSocket(out Peers[1], Config.Probe1);
      BindSocket(out Peers[2], Config.Probe2);

      if (Running) {
        Peers[0].Message_AddHandler<Protocol.NatProbe_TestEndPoint>(ClientQuery(Peers[0], 0));
        Peers[1].Message_AddHandler<Protocol.NatProbe_TestEndPoint>(ClientQuery(Peers[1], 1));
      }
    }

    protected override void OnUpdate() {
      Peers[0].Message_Recv(0);
      Peers[1].Message_Recv(0);
    }

    void BindSocket(out Protocol.Peer peer, UdpEndPoint endpoint) {
      var socket = Platform.CreateSocket(endpoint);

      if (socket.IsBound) {
        peer = new Protocol.Peer(socket);
        UdpLog.Info("bound nat-probe socket {0}", endpoint);
      }
      else {
        peer = null;
        Shutdown("nat-probe socket failed to bind to {1}: {2}", endpoint, socket.Error);
      }
    }

    Action<Protocol.NatProbe_TestEndPoint> ClientQuery(Protocol.Peer peer, int probe) {
      return qry => {
        Protocol.NatProbe_TestEndPoint_Result result;
        result = peer.Message_Create<Protocol.NatProbe_TestEndPoint_Result>();
        result.Probe = probe;
        result.Query = qry.MessageId;
        result.ClientWanEndPoint = qry.Sender;

        peer.Message_Send(result, result.ClientWanEndPoint);

        if (probe == PROBE0) {
          Protocol.NatProbe_TestUnsolicited test;
          test = peer.Message_Create<Protocol.NatProbe_TestUnsolicited>();
          test.ClientWanEndPoint = qry.Sender;
          test.Probe = probe;

          Peers[PROBE2].Message_Send(test, test.ClientWanEndPoint);
        }
      };
    }
  }
}
