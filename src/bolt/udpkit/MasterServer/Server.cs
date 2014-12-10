using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit.MasterServer {
  public class Server : NAT.Peer<MasterServer.Config> {
    public const uint HOST_TIMEOUT = 60 * 1000;

    Protocol.Peer ProtocolPeer;

    List<UdpSession> HostList = new List<UdpSession>();
    Dictionary<Guid, UdpSession> HostLookup = new Dictionary<Guid, UdpSession>();

    NAT.Probe.Server Probe;
    NAT.Punch.Server Punch;

    public Server(UdpPlatform platform)
      : base(platform) {
    }

    protected override void OnInit() {
      ProtocolPeer = new Protocol.Peer(Platform.CreateSocket(Config.Master));

      ProtocolPeer.Message_AddHandler<Protocol.MasterServer_HostRegister>(Host_Register_Query);
      ProtocolPeer.Message_AddHandler<Protocol.MasterServer_HostKeepAlive>(Host_KeepAlive);
      ProtocolPeer.Message_AddHandler<Protocol.MasterServer_NatProbeInfo>(NatProbeInfo_Query);
      ProtocolPeer.Message_AddHandler<Protocol.MasterServer_Session_ListRequest>(Session_ListRequest);
      ProtocolPeer.Message_AddHandler<Protocol.MasterServer_Introduce>(Introduce_Query);

      ProtocolPeer.Ack_AddHandler<Protocol.MasterServer_IntroduceInfo>(IntroduceInfo);

      Punch = new NAT.Punch.Server(Platform);
      Punch.Start(Config);

      Probe = new NAT.Probe.Server(Platform);
      Probe.Start(Config);
    }

    protected override void OnUpdate() {
      uint now = Platform.GetPrecisionTime();

      // receive new messages
      ProtocolPeer.Message_Recv(1);

      // update queries
      ProtocolPeer.Query_Update(now);

      // check timeouts
      Host_CheckTimeouts(now);
    }

    void Host_CheckTimeouts(uint now) {
      for (int i = 0; i < HostList.Count; ++i) {
        // if this host has timed out
        if (HostList[i].LastSeen + HOST_TIMEOUT < now) {
          // remove from lookup
          HostLookup.Remove(HostList[i].Id);

          // remove from list
          HostList.RemoveAt(i);

          // step back counter
          --i;
        }
      }
    }

    void Host_KeepAlive(Protocol.MasterServer_HostKeepAlive msg) {
      if (msg == null) {
        return;
      }

      UdpSession host;

      if (HostLookup.TryGetValue(msg.PeerId, out host)) {
        host.LastSeen = Platform.GetPrecisionTime();
      }
    }

    void Host_Register_Query(Protocol.MasterServer_HostRegister msg) {
      if (msg == null) {
        return;
      }

      UdpSession host;

      host = msg.Host;
      host.Id = msg.PeerId;
      host.WanEndPoint = msg.Sender;
      host.LastSeen = Platform.GetPrecisionTime();

      HostLookup[host.Id] = host;
      HostList.Add(host);

      UdpLog.Info("Host Registered: {0}", host);

      ProtocolPeer.Message_Ack(msg);
    }

    void Session_ListRequest(Protocol.MasterServer_Session_ListRequest msg) {
      foreach (var host in HostList) {
        if (host.ConnectivityStatus == UdpConnectivityStatus.Unknown) {
          continue;
        }

        Protocol.MasterServer_Session_Info reply;

        reply = ProtocolPeer.Message_Create<Protocol.MasterServer_Session_Info>();
        reply.Host = host;

        ProtocolPeer.Message_Send(reply, msg.Sender);
      }
    }

    void NatProbeInfo_Query(Protocol.MasterServer_NatProbeInfo query) {
      Protocol.MasterServer_NatProbeInfo_Result result;

      result = Platform.CreateMessage<Protocol.MasterServer_NatProbeInfo_Result>();
      result.Query = query.MessageId;
      result.Probe0 = Config.Probe0;
      result.Probe1 = Config.Probe1;
      result.Probe2 = Config.Probe2;

      ProtocolPeer.Message_Send(result, query.Sender);
    }

    void Introduce_Query(Protocol.MasterServer_Introduce query) {
      UdpSession host;

      Protocol.MasterServer_Introduce_Result result;

      result = ProtocolPeer.Message_Create<Protocol.MasterServer_Introduce_Result>();
      result.Query = query.MessageId;

      // make sure host still exists
      if (HostLookup.TryGetValue(query.Host.Id, out host)) {
        switch (host.ConnectivityStatus) {
          case UdpConnectivityStatus.DirectConnection:
            result.Status = Protocol.MasterServer_Introduce_Result_Status.DirectConnection;
            ProtocolPeer.Message_Send(result, query.Sender);
            break;

          case UdpConnectivityStatus.RequiresIntroduction:
            result.Status = Protocol.MasterServer_Introduce_Result_Status.IntroductionRequired;
            ProtocolPeer.Message_Send(result, query.Sender);

            Send_IntroduceInfo_Query(query.Sender, host.Id);
            Send_IntroduceInfo_Query(host.WanEndPoint, query.PeerId);

            break;

          case UdpConnectivityStatus.ReverseDirectConnection:
            break;
        }
      }
      else {
        result.Status = Protocol.MasterServer_Introduce_Result_Status.HostGone;
        ProtocolPeer.Message_Send(result, query.Sender);
      }
    }

    void Send_IntroduceInfo_Query(UdpEndPoint endpoint, Guid remote) {

      Protocol.MasterServer_IntroduceInfo msg;

      msg = ProtocolPeer.Message_Create<Protocol.MasterServer_IntroduceInfo>();
      msg.Remote = remote;
      msg.PunchServer = Config.Punch;

      ProtocolPeer.Message_Send(msg, endpoint);
    }

    void IntroduceInfo(Protocol.MasterServer_IntroduceInfo obj) {
    }
  }
}
