using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit {
  partial class NatProbe {
    class Server : Peer {
      struct ProbeState {
        public uint PacketsSent;
        public uint PacketsRecv;
        public DateTime SendTime;
        public UdpEndPoint WanEndPoint;
      }

      class ProbeClient {
        public DateTime CreatedTime;
        public readonly Guid Id;
        public readonly ProbeState[] State = new ProbeState[PROBE_COUNT];

        public ProbeClient(Guid id) {
          Id = id;
          CreatedTime = DateTime.Now;
        }
      }

      readonly byte[] Buffer = new byte[1024];
      readonly UdpPlatformSocket[] Sockets = new UdpPlatformSocket[PROBE_COUNT];
      readonly Dictionary<Guid, ProbeClient> Clients = new Dictionary<Guid, ProbeClient>();

      public Server(NatProbe probe)
        : base(probe) {
      }

      public override void Init() {
        for (int i = 0; i < Math.Min(PROBE_COUNT, Sockets.Length); ++i) {
          Sockets[i] = Probe.platform.CreateSocket();
          Sockets[i].Bind(Probe.Config.Servers[i]);

          if (Sockets[i].IsBound) {
            UdpLog.Info("bound nat-probe socket {0}", Sockets[i].EndPoint);
          }
          else {
            Probe.Shutdown("nat-probe socket-{0} failed to bind to {1}: {2}", i, Probe.Config.Servers[i], Sockets[i].Error);
            return;
          }
        }
      }

      public override void Update() {
        UpdateSocket(0, Sockets[0]);
        UpdateSocket(1, Sockets[1]);

        // update clients
        foreach (var client in Clients.Values) {
          UpdateClient(client);
        }
      }

      byte[] GetBuffer() {
        // always zero out buffer
        Array.Clear(Buffer, 0, Buffer.Length);

        return Buffer;
      }

      //void UpdateClient(ProbeClient client) {
      //  for (byte i = 0; i < client.State.Length; ++i) {
      //    var hasEndPoint = client.State[i].WanEndPoint != UdpEndPoint.Any;
      //    var sendExpired = client.State[i].SendTime.Add(Config.SendRate) < DateTime.Now;
      //    var canSendMore = client.State[i].PacketsSent < Probe.Config.PacketCount;

      //    if (hasEndPoint && sendExpired && canSendMore) {
      //      ProbeSocketSend(client, i);
      //    }
      //  }
      //}

      void ProbeSocketSend(ProbeClient client, byte probe) {
        if (Sockets[probe] != null) {
          int o = 0;
          var buffer = GetBuffer();

          buffer.PackByte(ref o, probe);
          buffer.PackGuid(ref o, client.Id);
          buffer.PackEndPointScrambled(ref o, client.State[probe].WanEndPoint);

          client.State[probe].SendTime = DateTime.Now;
          client.State[probe].PacketsSent += 1;

          Sockets[probe].SendTo(buffer, o, client.State[probe].WanEndPoint);
        }
      }

      void UpdateSocket(byte probe, UdpPlatformSocket socket) {
        if (socket != null) {
          if (socket.RecvPoll(0)) {
            var buffer = GetBuffer();
            var endpoint = new UdpEndPoint();
            var bytes = socket.RecvFrom(buffer, ref endpoint);
            var client = ReadClient(endpoint, buffer, bytes, true);

            if (client != null) {
              // count packets received
              client.State[probe].PacketsRecv += 1;

              if (client.State[probe].WanEndPoint == UdpEndPoint.Any) {
                // assign probes endpoint
                client.State[probe].WanEndPoint = endpoint;

                UdpLog.Info("client {0} probe-{1} endpoint is {2}", client.Id, probe, client.State[probe].WanEndPoint);

                // if this is probe 0, we should also set the end-point for probe 2
                if (probe == PROBE0) {
                  client.State[PROBE2].WanEndPoint = endpoint;
                }
              }
            }
          }
        }
      }

      ProbeClient ReadClient(UdpEndPoint endpoint, byte[] buffer, int bytes, bool allowCreate) {
        ProbeClient client = null;

        if (bytes > 16) {
          var o = 0;
          var clientProbe = buffer.ReadByte(ref o);
          var id = Buffer.ReadGuid(ref o);

          if (Clients.TryGetValue(id, out client) == false) {
            if (allowCreate) {
              UdpLog.Info("client {0} at {1} connected", id, endpoint);

              client = new ProbeClient(id);
              Clients.Add(client.Id, client);
            }
          }
        }

        return client;
      }
    }
  }
}
