using System;
using System.Collections.Generic;

using System.Text;
using System.Threading;

namespace UdpKit.NAT.Probe {
  public sealed class Client : Peer {
    class Session {
      public uint PacketsRecv;
      public uint PacketsSent;

      public DateTime SendTime;

      public UdpEndPoint ServerWanEndPoint;
      public UdpEndPoint ClientWanEndPointOnServer;

      public Session(UdpEndPoint serverEndpoint) {
        ServerWanEndPoint = serverEndpoint;
      }
    }

    DateTime startTime;
    volatile Result result;

    ManualResetEvent done;
    Session[] sessions = new Session[PROBE_COUNT];
    UdpPlatformSocket[] sockets = new UdpPlatformSocket[CLIENT_COUNT];

    public Result Result {
      get { return result; }
    }

    public ManualResetEvent Done {
      get { return done; }
    }

    public Client(UdpPlatform platform)
      : base(platform) {
      done = new ManualResetEvent(false);
    }

    protected override void OnInit() {
      result = default(Result);
      startTime = DateTime.Now;

      for (int i = 0; i < sockets.Length; ++i) {
        UdpPlatformSocket socket;

        socket = Platform.CreateSocket();
        socket.Bind(UdpEndPoint.Any);

        if (socket.IsBound) {
          sockets[i] = socket;
          UdpLog.Info("nat-probe socket-{0} bound to {1}", i, socket.EndPoint);
        }
        else {
          Shutdown("nat-probe socket-{0} binding failed: {1}", i, socket.Error);
          return;
        }
      }

      for (int i = 0; i < PROBE_COUNT; ++i) {
        sessions[i] = new Session(Config.Servers[i]);
      }
    }

    protected override void OnUpdate() {
      RecvPackets();

      SendPackets(sessions[0]);
      SendPackets(sessions[1]);

      if (startTime.AddSeconds(Config.SessionTimeout) < DateTime.Now) {
        Stop();
      }
    }

    public override void Stop() {
      if (result != Result.Failed) {
        if (sessions[0].PacketsRecv == 0 || sessions[1].PacketsRecv == 0) {
          result = Result.Failed;
        }
        else {
          var wan0 = sessions[0].ClientWanEndPointOnServer;
          var wan1 = sessions[1].ClientWanEndPointOnServer;

          if (wan0 == wan1) {
            result |= Result.EndPointPreservation;
          }
        }
      }

      base.Stop();
    }

    protected new void Shutdown(string format, params object[] args) {
      result = Result.Failed;
      base.Shutdown(format, args);
    }

    void RecvPackets() {
      if (sockets[0] != null) {
        while (sockets[0].RecvPoll(0)) {
          var endpoint = new UdpEndPoint();
          var buffer = GetBuffer();
          var bytes = sockets[0].RecvFrom(buffer, ref endpoint);

          var o = 0;
          var probeId = Blit.ReadByte(buffer, ref o);
          var clientId = Blit.ReadGuid(buffer, ref o);

          if (clientId == Platform.Id) {
            if (probeId < PROBE_COUNT) {
              sessions[probeId].PacketsRecv += 1;
              sessions[probeId].ClientWanEndPointOnServer = Blit.ReadEndPoint(buffer, ref o);

              UdpLog.Info("received wan-endpoint {0} from probe-{1}", sessions[probeId].ClientWanEndPointOnServer, probeId);

              if (probeId == PROBE2) {
                result |= Result.AllowsUnsolicitedTraffic;
              }
            }
            else {
              if (probeId == CLIENT1) {
                UdpLog.Info("hairpin translation support detected");
                result |= Result.HairpinTranslation;
              }
            }
          }
        }
      }
    }

    void SendPackets(Session session) {
      var sendExpired = session.SendTime.AddMilliseconds(Config.SendRate) < DateTime.Now;
      var canSendMore = session.PacketsSent < Config.PacketCount;

      if (sendExpired && canSendMore) {
        UdpLog.Info("sending probe message to {0}", session.ServerWanEndPoint);

        var buffer = GetBuffer();
        var o = 0;

        Blit.PackByte(buffer, ref o, CLIENT0);
        Blit.PackGuid(buffer, ref o, Platform.Id);

        sockets[0].SendTo(buffer, o, session.ServerWanEndPoint);

        session.PacketsSent += 1;
        session.SendTime = DateTime.Now;
      }
    }
  }
}
