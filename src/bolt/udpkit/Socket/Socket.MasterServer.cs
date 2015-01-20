using System;
using System.Collections.Generic;

using System.Text;

namespace UdpKit {
  partial class UdpSocket {
    const uint MASTERSERVER_UPDATERATE = 50;
    const uint MASTERSERVER_SESSION_LISTREQUEST_TIMEOUT = 500;

    const uint MASTERSERVER_HOST_REGISTER_TIMEOUT = 5000;
    const uint MASTERSERVER_HOST_KEEPALIVE_TIMEOUT = 20000;

    class MasterServerInfo {
      public uint LastSend;
      public uint LastUpdate;
      public UdpEndPoint EndPoint;
    }

    MasterServerInfo MasterServer = new MasterServerInfo();

    bool MasterServer_IsValid() {
      return (MasterServer != null) && (MasterServer.EndPoint != UdpEndPoint.Any);
    }

    void MasterServer_SetupProtocol() {
      protocol.SetHandler<Protocol.MasterServer_Session_Info>(MasterServer_Session_Info);
      protocol.SetHandler<Protocol.MasterServer_IntroduceInfo>(MasterServer_IntroduceInfo_Ack);

      protocol.SetCallback<Protocol.MasterServer_Introduce>(MasterServer_Introduce);
      protocol.SetCallback<Protocol.MasterServer_NatProbeInfo>(MasterServer_NatProbeInfo_Ack);
      protocol.SetCallback<Protocol.MasterServer_HostRegister>(MasterServer_HostRegister_Ack);
    }

    void MasterServer_Set(UdpEndPoint endpoint) {
      MasterServer = new MasterServerInfo();
      MasterServer.EndPoint = endpoint;

      // reset nat probe client
      NatProbe_Reset();

      // request nat probe info from new master server
      protocol.Send<Protocol.MasterServer_NatProbeInfo>(MasterServer.EndPoint);
    }

    void MasterServer_Send(Protocol.Message msg) {
      // send message
      protocol.Send(msg, MasterServer.EndPoint);

      // store last send time
      MasterServer.LastSend = GetCurrentTime();
    }

    void MasterServer_Update(uint now) {
      if (MasterServer_IsValid()) {
        switch (mode) {
          case UdpSocketMode.Host: MasterServer_UpdateHost(now); break;
          case UdpSocketMode.Client: MasterServer_UpdateClient(now); break;
        }
      }
    }

    void MasterServer_UpdateHost(uint now) {
      if ((MasterServer.LastSend + MASTERSERVER_HOST_KEEPALIVE_TIMEOUT) < now) {
        MasterServer_Send(platform.CreateMessage<Protocol.MasterServer_HostKeepAlive>());
      }

      //if (NAT_UPnP_Result != Session.Local.UPnP_Result) {
      //  UdpLog.Debug("NAT_UPnP_Result = {0}", NAT_UPnP_Result);
      //  Session.Local.UPnP_Result = NAT_UPnP_Result;
      //  MasterServer_HostRegister();
      //}

    }

    void MasterServer_UpdateClient(uint now) {

    }

    void MasterServer_Connect(UdpSession session, byte[] token) {
      // only allow wan sessions
      UdpAssert.Assert(session.HasWan);

      if (MasterServer.EndPoint == UdpEndPoint.Any) {
        return;
      }

      //// simple case where the host allows direct connections
      //if (session.ConnectivityStatus == UdpConnectivityStatus.DirectConnection) {
      //  ConnectToEndPoint(session.WanEndPoint, token);
      //  return;
      //}

      Protocol.MasterServer_Introduce query;

      query = protocol.Create<Protocol.MasterServer_Introduce>();
      query.Client = Session.Local;
      query.Host = session;

      MasterServer_Send(query);
    }

    void MasterServer_HostRegister() {
      if (MasterServer.EndPoint == UdpEndPoint.Any) {
        return;
      }

      Protocol.MasterServer_HostRegister query;

      query = protocol.Create<Protocol.MasterServer_HostRegister>();
      query.Host = Session.Local;

      MasterServer_Send(query);
    }

    void MasterServer_Session_ListRequest() {
      if (MasterServer.EndPoint == UdpEndPoint.Any) {
        return;
      }

      MasterServer_Send(protocol.Create<Protocol.MasterServer_Session_ListRequest>());
    }

    void MasterServer_NatProbeInfo_Ack(Protocol.MasterServer_NatProbeInfo query) {
      if (NatProbe_IsRunning() == false) {
        if (query.Result == null) {

        }
        else {
          NatProbe_Start(query.Result.Probe0, query.Result.Probe1, query.Result.Probe2);
        }
      }
    }

    void MasterServer_Session_Info(Protocol.MasterServer_Session_Info msg) {
      Session_Add(msg.Host);
    }

    void MasterServer_HostRegister_Ack(Protocol.MasterServer_HostRegister msg) {
      if (msg.Result == null) {
        MasterServer_HostRegister();
      }
    }

    void MasterServer_Introduce(Protocol.MasterServer_Introduce msg) {

    }

    void MasterServer_IntroduceInfo_Ack(Protocol.MasterServer_IntroduceInfo msg) {
      protocol.Ack(msg);

      Protocol.NatPunch_PeerRegister register;

      register = protocol.Create<Protocol.NatPunch_PeerRegister>();
      register.Remote = msg.Remote;

      protocol.Send(register, msg.PunchServer);
    }
  }
}
