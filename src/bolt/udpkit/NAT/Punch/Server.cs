using System;
using System.Collections.Generic;

using System.Text;

namespace UdpKit.NAT.Punch {
  public class Server : NAT.Peer<MasterServer.Config> {
    const uint INTRO_TIMEOUT = 30 * 1000;

    Protocol.Peer ProtocolPeer;

    struct PeerPair {
      public class EqualityComparer : IEqualityComparer<PeerPair> {
        public static readonly EqualityComparer Instance = new EqualityComparer();

        EqualityComparer() {

        }

        bool IEqualityComparer<PeerPair>.Equals(PeerPair x, PeerPair y) {
          return
            (x.Peer0 == y.Peer0) && (x.Peer1 == y.Peer1)
            ||
            (x.Peer0 == y.Peer1) && (x.Peer1 == y.Peer0);
        }

        int IEqualityComparer<PeerPair>.GetHashCode(PeerPair obj) {
          return obj.Peer0.GetHashCode() ^ obj.Peer1.GetHashCode();
        }
      }

      public readonly Guid Peer0;
      public readonly Guid Peer1;

      public PeerPair(Guid p0, Guid p1) {
        Peer0 = p0;
        Peer1 = p1;
      }
    }

    enum PeerState {
      New,
      Pinging,
      Ready,
    }

    struct PeerInfo {
      public UdpEndPoint Wan;
      public UdpEndPoint Lan;
      public PeerState State;
      public uint Ping;
    }

    class Introduction {
      public uint LastSeen;

      public PeerPair Pair;
      public PeerInfo[] Info = new PeerInfo[2];

      public int this[Guid peer] {
        get {
          if (Pair.Peer0 == peer) {
            return 0;
          }
          else if (Pair.Peer1 == peer) {
            return 1;
          }
          else {
            throw new InvalidOperationException();
          }
        }
      }
    }

    List<Introduction> Intro_List = new List<Introduction>();
    Dictionary<PeerPair, Introduction> Intro_Lookup = new Dictionary<PeerPair, Introduction>(PeerPair.EqualityComparer.Instance);

    public Server(UdpPlatform platform)
      : base(platform) {

    }

    protected override void OnInit() {
      ProtocolPeer = new Protocol.Peer(Platform.CreateSocket(Config.Punch));
      ProtocolPeer.SetHandler<Protocol.NatPunch_PeerRegister>(Peer_Register);
      ProtocolPeer.SetCallback<Protocol.Socket_Ping>(Socket_Ping);

      if (ProtocolPeer.Socket.IsBound == false) {
        Shutdown("failed to bind socket to {0}", Config.Punch);
      }
    }

    protected override void OnUpdate() {
      uint now = Platform.GetPrecisionTime();

      // receive new data
      ProtocolPeer.Recv(1);

      // update queries
      ProtocolPeer.Update(now);

      // update sessions
      Peer_Update(now);
    }

    void Peer_Update(uint now) {
      for (int i = 0; i < Intro_List.Count; ++i) {
        var p = Intro_List[i];

        if ((p.LastSeen + INTRO_TIMEOUT) < now) {
          // remove timed out peer
          Intro_List.RemoveAt(i);
          Intro_Lookup.Remove(p.Pair);

          // step index back
          --i;
        }
      }
    }

    void Peer_Register(Protocol.NatPunch_PeerRegister msg) {
      PeerPair pair = new PeerPair(msg.PeerId, msg.Remote);
      Introduction intro;

      if (Intro_Lookup.TryGetValue(pair, out intro) == false) {
        intro = new Introduction();
        intro.Pair = pair;

        Intro_List.Add(intro);
        Intro_Lookup.Add(intro.Pair, intro);
      }

      int i = intro[msg.PeerId];

      intro.LastSeen = ProtocolPeer.Platform.GetPrecisionTime();
      intro.Info[intro[msg.PeerId]].Lan = msg.Lan;
      intro.Info[intro[msg.PeerId]].Wan = msg.Sender;

      ProtocolPeer.Ack(msg);

      if (intro.Info[intro[msg.PeerId]].State == PeerState.New) {
        Protocol.Socket_Ping ping;

        ping = ProtocolPeer.Create<Protocol.Socket_Ping>();
        ping.Local = msg.PeerId;
        ping.Remote = msg.Remote;

        // send ping packet
        ProtocolPeer.Send(ping, msg.Sender);

        // we don't wanna do this 
        intro.Info[intro[msg.PeerId]].State = PeerState.Pinging;
      }
    }

    void Socket_Ping(Protocol.Socket_Ping msg) {
      PeerPair pair = new PeerPair(msg.Local, msg.Remote);
      Introduction intro;

      if (Intro_Lookup.TryGetValue(pair, out intro)) {
        uint ping = Math.Max(1u, Platform.GetPrecisionTime() - msg.SendTime);

        intro.Info[intro[msg.Local]].Ping = ping;
        intro.Info[intro[msg.Local]].State = PeerState.Ready;

        var ready0 = intro.Info[0].State == PeerState.Ready;
        var ready1 = intro.Info[1].State == PeerState.Ready;

        if (ready0 && ready1) {
          // both peers are ready, time to punch!

          Send_PunchInfo(intro, 0);
          Send_PunchInfo(intro, 1);
        }
      }
    }

    void Send_PunchInfo(Introduction intro, int peer) {
      int remote = 1 - peer;

      Protocol.NatPunch_PunchInfo msg;

      msg = ProtocolPeer.Create<Protocol.NatPunch_PunchInfo>();
      msg.PunchTo = intro.Info[remote].Wan;
      msg.Ping = intro.Info[peer].Ping;

      ProtocolPeer.Send(msg, intro.Info[peer].Wan);
    }
  }
}
