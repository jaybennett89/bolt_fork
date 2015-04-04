﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit {
  partial class UdpSocket {
    internal sealed class MasterClient : Protocol.ProtocolService {

      class NatProbeState {
        public uint Timeout;
        public Protocol.ProtocolClient Hairpin;

        public UdpEndPoint Probe0;
        public UdpEndPoint Probe1;
        public UdpEndPoint Probe2;

        public UdpEndPoint Probe0WanResponse;
        public UdpEndPoint Probe1WanResponse;
      }

      enum State {
        Disconnected,
        Connected,
      }

      class NatPunchTarget {
        public uint Time;
        public uint Count;
        public UdpEndPoint EndPoint;
      }

      class NatPunchRequest {
        public uint Time;
        public uint Count;
        public byte[] Token;
        public UdpSession Target;
      }

      uint ping;
      uint keepalive;

      State state;
      UdpEndPoint endpoint;

      NatFeatures natFeatures;
      NatProbeState natProbeState;
      NatPunchRequest natPunchRequest;

      readonly UdpSocket socket;
      readonly List<NatPunchTarget> natPunchTargets = new List<NatPunchTarget>();

      internal Protocol.GetZeusInfoResult InfoResult;

      public bool IsConnected {
        get { return endpoint.IsWan && (state >= State.Connected); }
      }

      public bool IsConnectedHost {
        get { return IsConnected && socket.sessionManager.IsHostWithName; }
      }

      public UdpEndPoint LocalWanEndPoint {
        get { return natFeatures == null ? UdpEndPoint.Any : natFeatures.WanEndPoint; }
      }

      public MasterClient(UdpSocket s, Protocol.ProtocolClient p)
        : base(p) {
        socket = s;
      }

      public void Update(uint now) {
        // update this client
        Client.Update(now);

        // update nat stuff
        UpdateNatProbe(now);
        UpdateNatPunch(now);

        // update host stuff
        KeepAlive(now);
      }

      void UpdateNatPunch(uint now) {
        // PUNCH REQUESTS

        if ((natPunchRequest != null) && (natPunchRequest.Time < now)) {
          // if we have done less than 10 requests
          if (++natPunchRequest.Count <= socket.Config.NatPunchRequestCount) {
            // perform once more, and then wait 2000 ms
            natPunchRequest.Time = now + socket.Config.NatPunchRequestInterval;

            // send request
            Send<Protocol.PunchRequest>(endpoint, m => m.Host = natPunchRequest.Target.Id);
          }
          else {
            natPunchRequest = null;
          }
        }

        // PUNCH ATTEMPTS

        for (int i = 0; i < natPunchTargets.Count; ++i) {
          NatPunchTarget target = natPunchTargets[i];

          if (target.Time < now) {
            Send<Protocol.Punch>(target.EndPoint);

            if (target.Count == socket.Config.NatPunchOnceCount) {
              // remove this
              natPunchTargets.RemoveAt(i);

              // step index back
              --i;
            }
            else {
              target.Count += 1;
              target.Time = now + socket.Config.NatPunchOnceInterval;
            }
          }
        }
      }

      void UpdateNatProbe(uint now) {
        if (natProbeState != null) {
          if (natProbeState.Hairpin != null) {
            natProbeState.Hairpin.Update(now);
          }

          if (natProbeState.Timeout < socket.GetCurrentTime()) {
            try {
              if (natProbeState.Hairpin != null) {
                natProbeState.Hairpin.Socket.Close();
                natProbeState.Hairpin = null;
              }
            }
            catch (Exception exn) {
              UdpLog.Error(exn.ToString());
            }

            // whops
            natProbeState = null;

            // connected!
            state = State.Connected;

            // tell the master server about this
            Send<Protocol.ProbeFeatures>(endpoint, m => m.NatFeatures = natFeatures);

            socket.Raise(new UdpEventMasterServerNatFeatures { Features = natFeatures.Clone() });
          }
        }
      }

      void KeepAlive(uint now) {
        if (IsConnected && (keepalive < now)) {
          Send<Protocol.PeerKeepAlive>(endpoint);
          keepalive = now + socket.Config.HostKeepAliveInterval;
        }
      }

      public void RequestZeusInfo() {
        if (IsConnected) {
          Send<Protocol.GetZeusInfo>(endpoint);
        }
      }

      public void RequestSessionList() {
        if (IsConnected) {
          // forget all sessions from zeus
          socket.sessionManager.ForgetSessions(UdpSessionSource.Zeus);

          // request list from zeus
          Send<Protocol.GetHostList>(endpoint);
        }
      }

      public void Disconnect(string error, params object[] args) {
        try {
          UdpLog.Error(error, args);
          UdpLog.Error("Disconnecting from masterserver at {0}", endpoint);
        }
        finally {
          Disconnect();
        }
      }

      public void Disconnect() {
        try {
          if (IsConnected) {
            // tell user we are leaving
            socket.Raise(new UdpEventMasterServerDisconnected { EndPoint = endpoint });

            // tell master server we are leaving
            Send<Protocol.PeerDisconnect>(endpoint);

            //
            ClearPunchRequest();
          }
          else {
            socket.Raise(new UdpEventMasterServerConnectFailed { EndPoint = endpoint });
          }
        }
        finally {
          socket.masterClient = null;
          socket.sessionManager = new SessionManager(socket);
        }
      }

      public void Connect(UdpEndPoint ep) {
        if (socket.platform.SupportsMasterServer == false) {
          UdpLog.Error("The current platform: {0}, does not support the Bolt master server", socket.platform.GetType().Name);
          return;
        }

        if (endpoint != ep) {
          // connect to new one
          endpoint = ep;

          // setup callbacks
          Client.SetCallback<Protocol.PeerConnect>(AckPeerConnect);
          Client.SetCallback<Protocol.ProbeEndPoint>(AckProbeEndPoint);
          Client.SetCallback<Protocol.ProbeFeatures>(AckProbeFeatures);
          Client.SetCallback<Protocol.HostRegister>(AckHostRegister);
          Client.SetCallback<Protocol.GetHostList>(AckGetHostList);
          Client.SetCallback<Protocol.GetZeusInfo>(AckGetZeusInfo);

          // setup handlers
          Client.SetHandler<Protocol.ProbeUnsolicited>(OnProbeUnsolicited);
          Client.SetHandler<Protocol.ProbeHairpin>(OnProbeHairpin);
          Client.SetHandler<Protocol.HostInfo>(OnHostInfo);
          Client.SetHandler<Protocol.PunchOnce>(OnPunchOnce);
          Client.SetHandler<Protocol.Punch>(OnPunch);
          Client.SetHandler<Protocol.Error>(OnError);
          Client.SetHandler<Protocol.PeerReconnect>(OnPeerReconnect);

          Client.SetHandler<Protocol.DirectConnectionWan>(OnDirectConnectionWan);
          Client.SetHandler<Protocol.DirectConnectionLan>(OnDirectConnectionLan);

          // setup
          keepalive = socket.GetCurrentTime();

          // send peer connect
          Send<Protocol.PeerConnect>(endpoint);
        }
      }

      void OnPeerReconnect(Protocol.PeerReconnect perform) {
        if (socket.masterClient != null) {
          socket.masterClient.Disconnect();
        }

        socket.sessionManager = new SessionManager(socket);

        socket.masterClient = new MasterClient(socket, new Protocol.ProtocolClient(socket.PlatformSocket, socket.GameId, socket.PeerId));
        socket.masterClient.Connect(endpoint);
      }

      public void RegisterHost() {
        if (IsConnectedHost) {
          Send<Protocol.HostRegister>(endpoint, m => m.Host = socket.sessionManager.GetLocalSession());
        }
      }

      public void ConnectToSession(UdpSession session, byte[] token) {
        ClearPunchRequest();

        natPunchRequest = new NatPunchRequest {
          Count = 0,
          Time = 0,
          Target = session,
          Token = token
        };
      }

      void AckPeerConnect(Protocol.PeerConnect connect) {
        if (connect.Failed) {
          // tell user this happened
          socket.Raise(new UdpEventMasterServerConnectFailed { EndPoint = endpoint });

          // disconnect us
          Disconnect("Could not connect to master server at {0}", connect.Target);
          return;
        }

        UpdatePing(connect);

        natFeatures = new NatFeatures();
        natFeatures.LanEndPoint = socket.LANEndPoint;

        natProbeState = new NatProbeState();
        natProbeState.Probe0 = connect.Result.Probe0;
        natProbeState.Probe1 = connect.Result.Probe1;
        natProbeState.Probe2 = connect.Result.Probe2;
        natProbeState.Timeout = socket.GetCurrentTime() + socket.Config.NatProbeEndPointTimeout;

        Send<Protocol.ProbeEndPoint>(natProbeState.Probe0);
        Send<Protocol.ProbeEndPoint>(natProbeState.Probe1);

        Send<Protocol.GetZeusInfo>(endpoint);

        UdpLog.Info("Connected to master server at {0}, initiating NAT probe", connect.Target);

      }

      void AckProbeEndPoint(Protocol.ProbeEndPoint probe) {
        if (natProbeState != null) {
          if (probe.Result.Sender == natProbeState.Probe0) {
            natProbeState.Probe0WanResponse = probe.Result.WanEndPoint;
          }

          if (probe.Result.Sender == natProbeState.Probe1) {
            natProbeState.Probe1WanResponse = probe.Result.WanEndPoint;
          }

          if (natProbeState.Probe0WanResponse != UdpEndPoint.Any && natProbeState.Probe1WanResponse != UdpEndPoint.Any) {
            if (natProbeState.Probe0WanResponse == natProbeState.Probe1WanResponse) {
              UdpLog.Info("NAT Probe: SupportsEndPointPreservation:YES");

              // we support end point preservation
              natFeatures.SupportsEndPointPreservation = NatFeatureStates.Yes;
              natFeatures.WanEndPoint = natProbeState.Probe0WanResponse;

              // update wan endpoint for us
              socket.WANEndPoint = natFeatures.WanEndPoint;

              // begin hairpin test
              StartHairpinTest();
            }
            else {
              UdpLog.Info("NAT Probe: SupportsEndPointPreservation:NO PROBE:0{0} PROBE:1{1}", natProbeState.Probe0WanResponse, natProbeState.Probe1WanResponse);

              natFeatures.SupportsEndPointPreservation = NatFeatureStates.No;
              natFeatures.WanEndPoint = UdpEndPoint.Any;
              socket.WANEndPoint = UdpEndPoint.Any;

              // begin hairpin test
              StartHairpinTest();
            }
          }
        }
      }

      void StartHairpinTest() {
        natProbeState.Hairpin = new Protocol.ProtocolClient(socket.platform.CreateSocket(UdpEndPoint.Any), socket.GameId, socket.PeerId);
        natProbeState.Hairpin.Send(natProbeState.Hairpin.CreateMessage<Protocol.ProbeHairpin>(), natFeatures.WanEndPoint);

        natProbeState.Timeout = socket.GetCurrentTime() + socket.Config.NatProbeHairpinTimeout;
      }

      void AckProbeFeatures(Protocol.ProbeFeatures features) {
        if (features.Failed) {
          Disconnect("Failed to register NAT probe result with master server");
          return;
        }

        // tell user about this
        socket.Raise(new UdpEventMasterServerConnected { EndPoint = endpoint });

        // Update ping
        UpdatePing(features);

        // assign wan endpoint to local session
        socket.sessionManager.SetWanEndPoint(features.NatFeatures.WanEndPoint);

        // if we're a host register us
        RegisterHost();
      }

      void AckHostRegister(Protocol.HostRegister obj) {
        if (obj.Failed) {
          Disconnect("Could not register host with master server at {0}", obj.Target);
          return;
        }

        UpdatePing(obj);

        UdpLog.Info("Successfully registered with master server");
      }

      void AckGetZeusInfo(Protocol.GetZeusInfo info) {
        if (info.Failed) {
          UdpLog.Error("Could not get server info from zeus");
          return;
        }

        InfoResult = info.Result;
      }

      void AckGetHostList(Protocol.GetHostList obj) {
        if (obj.Failed) {
          Disconnect("Could not get session list from master server at {0}", obj.Target);
          return;
        }

        UpdatePing(obj);
      }

      void OnProbeUnsolicited(Protocol.ProbeUnsolicited probe) {
        if (natProbeState != null) {
          if (probe.Sender == natProbeState.Probe2) {
            UdpLog.Info("NAT Probe: AllowsUnsolicitedTraffic");
            natFeatures.AllowsUnsolicitedTraffic = NatFeatureStates.Yes;
          }
        }
      }

      void OnProbeHairpin(Protocol.ProbeHairpin obj) {
        if (natProbeState != null) {
          if (obj.PeerId == socket.PeerId) {
            UdpLog.Info("NAT Probe: SupportsHairpinTranslation");
            natFeatures.SupportsHairpinTranslation = NatFeatureStates.Yes;
            natProbeState.Timeout = 0;
          }
        }
      }

      void OnHostInfo(Protocol.HostInfo obj) {
        socket.sessionManager.UpdateSession(obj.Host, UdpSessionSource.Zeus);
      }

      void OnPunchOnce(Protocol.PunchOnce once) {
        uint time = (socket.GetCurrentTime() + socket.Config.NatPunchOnceDelay) - ping;

        foreach (var t in natPunchTargets) {
          if (t.EndPoint == once.RemoteEndPoint && t.Count > 0) {
            t.Time = time;
            return;
          }
        }

        if (socket.Mode == UdpSocketMode.Client) {
          natPunchTargets.Clear();
        }

        natPunchTargets.Add(new NatPunchTarget { Time = time, Count = 0, EndPoint = once.RemoteEndPoint });
      }

      void OnDirectConnectionLan(Protocol.DirectConnectionLan direct) {
        ClearPunchRequest();

        //
        socket.OnEventConnect(new UdpEventConnectEndPoint { EndPoint = direct.RemoteEndPoint, Token = natPunchRequest.Token });
      }

      void OnDirectConnectionWan(Protocol.DirectConnectionWan direct) {
        ClearPunchRequest();

        //
        socket.OnEventConnect(new UdpEventConnectEndPoint { EndPoint = direct.RemoteEndPoint, Token = natPunchRequest.Token });
      }

      void OnPunch(Protocol.Punch punch) {
        if ((socket.Mode == UdpSocketMode.Client) && (natPunchTargets.Any(x => x.EndPoint == punch.Sender))) {
          //
          ClearPunchRequest();

          //
          socket.OnEventConnect(new UdpEventConnectEndPoint { EndPoint = punch.Sender, Token = natPunchRequest.Token });
        }
      }

      void ClearPunchRequest() {
        natPunchRequest = null;
        natPunchTargets.Clear();
      }

      void OnError(Protocol.Error obj) {
        UdpLog.Error("Received Error From Zeus: {0}", obj.Text);
      }

      void UpdatePing(Protocol.Query query) {
        var rtt = socket.GetCurrentTime() - query.SendTime;
        if (rtt == 0) {
          ping = rtt;
        }
        else {
          ping = (ping + rtt) / 2;
        }
      }
    }
  }
}
