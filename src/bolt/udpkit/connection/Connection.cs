using System;
using System.Collections.Generic;

namespace UdpKit {
  enum UdpConnectionState : int {
    None = 0,
    Connecting = 1,
    Connected = 2,
    Disconnected = 3,
    Destroy = 4
  }

  enum UdpConnectionError : int {
    None = 0,
    SequenceOutOfBounds = 1,
    IncorrectCommand = 2,
    SendWindowFull = 3,
    UnknownStreamChannel = 4,
    InvalidBlockNumber = 5,
  }

  enum UdpConnectionMode : int {
    Client = 1,
    Server = 2
  }

  public partial class UdpConnection {
    float NetworkRtt = 0.1f;
    float AliasedRtt = 0.1f;

    uint ConnectTimeout;
    uint ConnectAttempts;

    uint StreamSendInterval = 50;
    Dictionary<UdpChannelName, UdpChannelStreamer> StreamChannels;

    internal UdpPipe PacketPipe;
    internal UdpPipe StreamPipe;

    readonly UdpEndPoint EndPoint;
    readonly UdpConnectionMode Mode;

    internal UdpSocket Socket;
    internal UdpConnectionState State;

    internal uint SendTime { get; private set; }
    internal uint RecvTime { get; private set; }

    internal uint ConnectionId;

    internal byte[] ConnectToken;
    internal byte[] AcceptToken;
    internal byte[] AcceptTokenWithPrefix;

    /// <summary>
    /// A user-assignable object
    /// </summary>
    public object UserToken {
      get;
      set;
    }

    /// <summary>
    /// The round-trip time of the network layer, excluding processing delays and ack time
    /// </summary>
    public float NetworkPing {
      get { return NetworkRtt; }
    }

    /// <summary>
    /// The total round-trip time, including processing delays and ack
    /// </summary>
    public float AliasedPing {
      get { return AliasedRtt; }
    }

    /// <summary>
    /// If this connection is a client
    /// </summary>
    public bool IsClient {
      get { return Mode == UdpConnectionMode.Client; }
    }

    /// <summary>
    /// IF this connections a server
    /// </summary>
    public bool IsServer {
      get { return Mode == UdpConnectionMode.Server; }
    }

    /// <summary>
    /// If we are connected
    /// </summary>
    public bool IsConnected {
      get { return State == UdpConnectionState.Connected; }
    }

    /// <summary>
    /// The remote end point
    /// </summary>
    public UdpEndPoint RemoteEndPoint {
      get { return EndPoint; }
    }

    /// <summary>
    /// How much of the current outgoing packet window is waiting for acks
    /// </summary>
    public float WindowFillRatio {
      get { return PacketPipe.FillRatio; }
    }

    internal UdpConnection(UdpSocket s, UdpConnectionMode m, UdpEndPoint ep) {
      Mode = m;
      Socket = s;
      EndPoint = ep;
      RecvTime = s.GetCurrentTime();

      NetworkRtt = Socket.Config.DefaultNetworkPing;
      AliasedRtt = Socket.Config.DefaultAliasedPing;

      State = UdpConnectionState.Connecting;

      PacketPipe = new UdpPipe(this, Socket.PacketPipeConfig);
      StreamPipe = new UdpPipe(this, Socket.StreamPipeConfig);
      StreamChannels = new Dictionary<UdpChannelName, UdpChannelStreamer>(UdpChannelName.EqualityComparer.Instance);
    }

    /// <summary>
    /// Send an object on this connection
    /// </summary>
    /// <param name="packet">The object to send</param>
    public void Send(UdpPacket packet) {
      Socket.Raise(UdpEvent.INTERNAL_SEND, this, packet);
    }

    /// <summary>
    /// Disconnect this connection forcefully
    /// </summary>
    public void Disconnect(byte[] token) {
      UdpEvent ev = new UdpEvent();
      ev.Type = UdpEvent.INTERNAL_DISCONNECT;
      ev.Connection = this;
      ev.DisconnectToken = token;
      Socket.Raise(ev);
    }

    internal void Lost(UdpPipe pipe, object obj) {
      if (obj != null) {
        switch (pipe.Id) {
          case UdpPipe.PIPE_PACKET:
            Socket.Raise(UdpEvent.PUBLIC_PACKET_LOST, this, (UdpPacket)obj);
            break;

          case UdpPipe.PIPE_STREAM:
            OnStreamLost((UdpStreamOpBlock)obj);
            break;
        }
      }
    }

    internal void Delivered(UdpPipe pipe, object obj) {
      if (obj != null) {
        switch (pipe.Id) {
          case UdpPipe.PIPE_PACKET:
            Socket.Raise(UdpEvent.PUBLIC_PACKET_DELIVERED, this, (UdpPacket)obj);
            break;

          case UdpPipe.PIPE_STREAM:
            OnStreamDelivered((UdpStreamOpBlock)obj);
            break;
        }
      }
    }

    internal void ProcessConnectingTimeouts(uint now) {
      switch (Mode) {
        case UdpConnectionMode.Client:
          if (ConnectTimeout < now && !SendCommandConnect()) {
            Socket.Raise(UdpEvent.PUBLIC_CONNECT_FAILED, EndPoint);

            // destroy this connection on next timeout check
            ChangeState(UdpConnectionState.Destroy);
          }
          break;
      }
    }

    internal void ProcessConnectedTimeouts(uint now) {
      if ((RecvTime + Socket.Config.ConnectionTimeout) < now) {
        UdpLog.Debug("{0} timed out", EndPoint);
        ChangeState(UdpConnectionState.Disconnected);
      }

      if (CheckState(UdpConnectionState.Connected)) {
        if ((SendTime + Socket.Config.PingTimeout) < now) {
          SendCommand(COMMAND_PING);
        }

        if ((StreamPipe.LastSend + StreamSendInterval) < now) {
          SendStream();
        }

        PacketPipe.CheckTimeouts(now);
        StreamPipe.CheckTimeouts(now);
      }
    }

    internal void ChangeState(UdpConnectionState newState) {
      ChangeState(newState, null);
    }

    internal void ChangeState(UdpConnectionState newState, byte[] token) {
      if (newState == State)
        return;

      UdpConnectionState oldState = State;

      switch (State = newState) {
        case UdpConnectionState.Connected:
          OnStateConnected(oldState);
          break;

        case UdpConnectionState.Disconnected:
          OnStateDisconnected(oldState, token);
          break;
      }
    }

    internal bool CheckState(UdpConnectionState stateValue) {
      return State == stateValue;
    }

    internal void UpdatePing(uint recvTime, uint sendTime, uint ackTime) {
      uint aliased = recvTime - sendTime;
      AliasedRtt = (AliasedRtt * 0.9f) + ((float)aliased / 1000f * 0.1f);

      uint network = aliased - UdpMath.Clamp(ackTime, 0, aliased);
      NetworkRtt = (NetworkRtt * 0.9f) + ((float)network / 1000f * 0.1f);
    }

    internal void ConnectionError(UdpConnectionError error) {
      ConnectionError(error, "");
    }

    internal void ConnectionError(UdpConnectionError error, string message) {
      UdpLog.Error("{1} error {0}: '{2}'", error, EndPoint, message);
      ChangeState(UdpConnectionState.Disconnected);
    }

    internal void UpdateSendTime() {
      SendTime = Socket.GetCurrentTime();
    }

    internal void Destroy() {
    }

    void OnStateConnected(UdpConnectionState oldState) {
      if (oldState == UdpConnectionState.Connecting) {
        UdpLog.Info("{0} connected", EndPoint);

        if (IsServer) {
          SendCommand(COMMAND_ACCEPTED, AcceptTokenWithPrefix);
        }

        Socket.Raise(UdpEvent.PUBLIC_CONNECTED, this);
      }
    }

    void OnStateDisconnected(UdpConnectionState oldState, byte[] token) {
      if (oldState == UdpConnectionState.Connected) {
        UdpLog.Info("{0} disconnected", RemoteEndPoint);

        PacketPipe.Disconnected();

        UdpEvent ev = new UdpEvent();
        ev.Type = UdpEvent.PUBLIC_DISCONNECTED;
        ev.Connection = this;
        ev.DisconnectToken = token;
        Socket.Raise(ev);
      }
    }

    void EnsureClientIsConnected() {
      if (IsClient && State == UdpConnectionState.Connecting && Socket.Config.AllowImplicitAccept) {
        ChangeState(UdpConnectionState.Connected);
      }
    }
  }
}
