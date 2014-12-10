using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit {
  partial class NatProbe {
    class Client : Peer {

      class ProbeServer {
        public uint PacketsSent;
        public DateTime SendTime;
        public UdpEndPoint ClientWanOnServer;

        public readonly DateTime CreatedTime;
        public readonly UdpEndPoint WanEndPoint;

        public ProbeServer(UdpEndPoint serverEndpoint) {
          WanEndPoint = serverEndpoint;
          CreatedTime = DateTime.Now;
        }
      }

      int hairpinPackets = 0;

      readonly Guid Id = Guid.NewGuid();
      readonly byte[] Buffer = new byte[1024];
      readonly ProbeServer[] Servers = new ProbeServer[PROBE_COUNT];
      readonly UdpPlatformSocket[] Sockets = new UdpPlatformSocket[2];

      byte[] GetBuffer() {
        // always zero out buffer
        Array.Clear(Buffer, 0, Buffer.Length);

        return Buffer;
      }

      public Client(NatProbe probe)
        : base(probe) {
      }

      public override void Init() {
        Probe.result = NatProbeResult.Timeout;

        for (int i = 0; i < Sockets.Length; ++i) {
          UdpPlatformSocket socket;

          socket = Probe.platform.CreateSocket();
          socket.Bind(UdpEndPoint.Any);

          if (socket.IsBound) {
            Sockets[i] = socket;
            UdpLog.Info("nat-probe socket-{0} bound to {1}", i, socket.EndPoint);
          }
          else {
            Probe.Shutdown("nat-probe socket-{0} binding failed: {1}", i, socket.Error);
            return;
          }
        }

        for (int i = 0; i < Math.Min(Servers.Length, Probe.Config.Servers.Length); ++i) {
          Servers[i] = new ProbeServer(Probe.Config.Servers[i]);
        }
      }

      public override void Update() {
        RecvPackets();

        SendPackets(Servers[0]);
        SendPackets(Servers[1]);

        if (hairpinPackets < Probe.Config.PacketCount) {
          if (Servers[0].ClientWanOnServer != UdpEndPoint.Any) {
            SendHairpinPacket(Servers[0].ClientWanOnServer);
          }
        }

        if (Servers[0].ClientWanOnServer != UdpEndPoint.Any && Servers[1].ClientWanOnServer != UdpEndPoint.Any) {
          if (Servers[0].ClientWanOnServer == Servers[1].ClientWanOnServer) {
            if ((Probe.result & NatProbeResult.EndPointPreservation) != NatProbeResult.EndPointPreservation) {
              UdpLog.Info("endpoint preservation detected");
            }

            Probe.result |= NatProbeResult.EndPointPreservation;
          }
        }

        if (Servers[0].PacketsSent == Probe.Config.PacketCount && Servers[1].PacketsSent == Probe.Config.PacketCount) {
          if (Probe.result != NatProbeResult.Timeout) {
            Probe.result &= ~NatProbeResult.Timeout;
          }

          Probe.Stop();
        }
      }

      void RecvPackets() {
        if (Sockets[0] != null) {
          while (Sockets[0].RecvPoll(0)) {
            var endpoint = new UdpEndPoint();
            var buffer = GetBuffer();
            var bytes = Sockets[0].RecvFrom(buffer, ref endpoint);

            var o = 0;
            var probeId = buffer.ReadByte(ref o);
            var clientId = buffer.ReadGuid(ref o);

            if (clientId == Id) {
              if (probeId < PROBE_COUNT) {
                Servers[probeId].ClientWanOnServer = buffer.ReadEndPointScrambled(ref o);
                UdpLog.Info("received wan-endpoint {0} from probe-{1}", Servers[probeId].ClientWanOnServer, probeId);

                if (probeId == PROBE2) {
                  Probe.result |= NatProbeResult.AllowsUnsolicitedTraffic;
                }
              }
              else {
                if (probeId == CLIENT1) {
                  UdpLog.Info("hairpin translation support detected");
                  Probe.result |= NatProbeResult.HairpinTranslation;
                }
              }
            }
          }
        }
      }

      void SendPackets(ProbeServer server) {
        var sendExpired = server.SendTime.AddMilliseconds(Probe.Config.SendRate) < DateTime.Now;
        var canSendMore = server.PacketsSent < Probe.Config.PacketCount;

        if (sendExpired && canSendMore) {
          UdpLog.Info("sending probe message to {0}", server.WanEndPoint);

          var buffer = GetBuffer();
          var o = 0;

          buffer.PackByte(ref o, CLIENT0);
          buffer.PackGuid(ref o, Id);

          Sockets[0].SendTo(buffer, o, server.WanEndPoint);

          server.PacketsSent += 1;
          server.SendTime = DateTime.Now;
        }
      }

      void SendHairpinPacket(UdpEndPoint endpoint) {
        UdpLog.Info("sending hairpin probe message to {0}", endpoint);

        var buffer = GetBuffer();
        var o = 0;

        buffer.PackByte(ref o, CLIENT1);
        buffer.PackGuid(ref o, Id);

        Sockets[1].SendTo(buffer, o, endpoint);

        ++hairpinPackets;
      }
    }
  }
}
