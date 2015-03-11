using System;
using System.Collections.Generic;
using System.Threading;

namespace UdpKit {

  public enum UdpSocketState : int {
    Created = 0,
    Running = 1,
    Shutdown = 2
  }

  public enum UdpSocketMode : int {
    None = 0,
    Host = 1,
    Client = 2
  }

  public partial class UdpSocket {
    readonly internal UdpConfig Config;
    readonly internal UdpPipeConfig PacketPipeConfig;
    readonly internal UdpPipeConfig StreamPipeConfig;

    internal UdpEndPoint LANEndPoint;
    internal UdpEndPoint WANEndPoint;

    readonly internal Guid PeerId;
    readonly internal Guid GameId;

    volatile int frame;
    volatile int channelIdCounter;
    volatile uint connectionIdCounter;
    volatile UdpSocketMode mode;
    volatile UdpSocketState state;


    readonly byte[] sendBuffer;
    readonly byte[] recvBuffer;

    readonly Thread thread;
    readonly UdpPlatform platform;
    readonly UdpPlatformSocket platformSocket;
    readonly UdpPacketPool packetPool;

    internal MasterClient masterClient;

    SessionManager sessionManager;
    BroadcastManager broadcastManager;

    readonly Queue<UdpEvent> eventQueueIn;
    readonly Queue<UdpEvent> eventQueueOut;

    readonly List<UdpConnection> connectionList = new List<UdpConnection>();
    readonly Dictionary<UdpEndPoint, byte[]> pendingConnections = new Dictionary<UdpEndPoint, byte[]>(new UdpEndPoint.Comparer());
    readonly Dictionary<UdpEndPoint, UdpConnection> connectionLookup = new Dictionary<UdpEndPoint, UdpConnection>(new UdpEndPoint.Comparer());
    readonly Dictionary<UdpChannelName, UdpStreamChannel> streamChannels = new Dictionary<UdpChannelName, UdpStreamChannel>(UdpChannelName.EqualityComparer.Instance);

    public UdpEndPoint SocketEndPoint {
      get { return platformSocket.EndPoint; }
    }

    /// <summary>
    /// LAN endpoint of this socket
    /// </summary>
    public UdpEndPoint LanEndPoint {
      get { return LANEndPoint; }
    }

    /// <summary>
    /// WAN endpoint of this socket
    /// </summary>
    public UdpEndPoint WanEndPoint {
      get { return WANEndPoint; }
    }

    /// <summary>
    /// The current state of the socket
    /// </summary>
    public UdpSocketState State {
      get { return state; }
    }

    /// <summary>
    /// The current mode of the socket
    /// </summary>
    public UdpSocketMode Mode {
      get { return mode; }
    }

    /// <summary>
    /// The precision time (in ms) of the underlying socket platform
    /// </summary>
    public uint PrecisionTime {
      get { return GetCurrentTime(); }
    }

    /// <summary>
    /// Current packet pool for this socket
    /// </summary>
    public UdpPacketPool PacketPool {
      get { return packetPool; }
    }

    /// <summary>
    /// A user-assignable object
    /// </summary>
    public object UserToken {
      get;
      set;
    }

    internal int ZeusInfoHosts {
      get { return ConnectedToMaster && masterClient.InfoResult != null ? masterClient.InfoResult.Hosts : 0; }
    }

    internal int ZeusInfoClientsInZeus {
      get { return ConnectedToMaster && masterClient.InfoResult != null ? masterClient.InfoResult.ClientsInZeus : 0; }
    }

    internal int ZeusInfoClientsInGames {
      get { return ConnectedToMaster && masterClient.InfoResult != null ? masterClient.InfoResult.ClientsInGames : 0; }
    }

    internal bool ConnectedToMaster {
      get { return masterClient != null && masterClient.IsConnected; }
    }

    public Func<int, byte[]> UnconnectedBufferProvider {
      get;
      set;
    }

    public UdpPlatformSocket PlatformSocket {
      get { return platformSocket; }
    }

    public UdpSocket(Guid gameId, UdpPlatform platform)
      : this(gameId, platform, new UdpConfig()) {
    }

    public UdpSocket(Guid gameId, UdpPlatform p, UdpConfig cfg) {
      GameId = gameId;
      PeerId = Guid.NewGuid();

      // set default values
      frame = 0;
      channelIdCounter = 0;
      connectionIdCounter = 1;

      // duplicate config object
      Config = cfg.Duplicate();

      // assign platform and create socket
      platform = p;
      platformSocket = platform.CreateSocket();

#if DEBUG
      if (this.Config.NoiseFunction == null) {
        Random random = new Random();
        this.Config.NoiseFunction = delegate() { return (float)random.NextDouble(); };
      }
#endif

      // allocate buffers
      sendBuffer = new byte[Math.Max(cfg.StreamDatagramSize, cfg.PacketDatagramSize) * 2];
      recvBuffer = new byte[Math.Max(cfg.StreamDatagramSize, cfg.PacketDatagramSize) * 2];

      // pools & queues
      packetPool = new UdpPacketPool(this);
      eventQueueIn = new Queue<UdpEvent>(4096);
      eventQueueOut = new Queue<UdpEvent>(4096);

      // setup packet pipe configuration
      PacketPipeConfig = new UdpPipeConfig {
        PipeId = UdpPipe.PIPE_PACKET,
        Timeout = 0, // don't use timeout
        AckBytes = 8,
        SequenceBytes = 2,
        UpdatePing = true,
        WindowSize = Config.PacketWindow,
        DatagramSize = Config.PacketDatagramSize,
      };

      // setup stream pipe config
      StreamPipeConfig = new UdpPipeConfig {
        PipeId = UdpPipe.PIPE_STREAM,
        Timeout = 500,
        AckBytes = 32,
        SequenceBytes = 3,
        UpdatePing = false,
        WindowSize = Config.StreamWindow,
        DatagramSize = Config.StreamDatagramSize
      };

      sessionManager = new SessionManager(this);
      sessionManager.SetConnections(0, Config.ConnectionLimit);

      broadcastManager = new BroadcastManager(this);

      // socket is created
      state = UdpSocketState.Created;

      // last thing we do is start the thread
      thread = new Thread(NetworkLoop);
      thread.Name = "UdpKit Thread";
      thread.IsBackground = true;
      thread.Start();
    }

    /// <summary>
    /// Start this socket
    /// </summary>
    /// <param name="endpoint">The endpoint to bind to</param>
    public void Start(UdpEndPoint endpoint, ManualResetEvent resetEvent, UdpSocketMode mode) {
      Raise(new UdpEventStart { EndPoint = endpoint, Mode = mode, ResetEvent = resetEvent });
    }

    /// <summary>
    /// Close this socket
    /// </summary>
    public void Close(ManualResetEvent resetEvent) {
      Raise(new UdpEventClose { ResetEvent = resetEvent });
    }

    public void Connect(UdpSession session, byte[] token) {
      Raise(new UdpEventSessionConnect { Session = session, Token = token });
    }

    /// <summary>
    /// Connect to remote endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint to connect to</param>
    public void Connect(UdpEndPoint endpoint, byte[] token) {
      Raise(new UdpEventConnectEndPoint { EndPoint = endpoint, Token = token });
    }

    /// <summary>
    /// Cancel ongoing attempt to connect to endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint to cancel connect attempt to</param>
    public void CancelConnect(UdpEndPoint endpoint) {
      Raise(new UdpEventConnectEndPointCancel { EndPoint = endpoint });
    }

    /// <summary>
    /// Accept a connection
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="userObject"></param>
    /// <param name="token"></param>
    public void Accept(UdpEndPoint endpoint, object userObject, byte[] token) {
      Raise(new UdpEventAcceptConnect { EndPoint = endpoint, Token = token, UserObject = userObject });
    }

    /// <summary>
    /// Refuse a connection request from a remote endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint to refuse</param>
    public void Refuse(UdpEndPoint endpoint, byte[] token) {
      Raise(new UdpEventRefuseConnect { EndPoint = endpoint, Token = token });
    }

    /// <summary>
    /// A list of all currently available sessions
    /// </summary>
    public UdpSession[] GetSessions() {
      return new UdpSession[0];
    }

    /// <summary>
    /// Poll socket for any events
    /// </summary>
    /// <param name="ev">The current event on this socket</param>
    /// <returns>True if a new event is available, False otherwise</returns>
    public bool Poll(out UdpEvent ev) {
      lock (eventQueueOut) {
        if (eventQueueOut.Count > 0) {
          ev = eventQueueOut.Dequeue();
          return true;
        }
      }

      ev = default(UdpEvent);
      return false;
    }

    public void MasterServerDisconnect() {
      UdpEvent ev = new UdpEvent();
      ev.Type = UdpEvent.INTERNAL_MASTERSERVER_DISCONNECT;
      Raise(ev);
    }

    public void MasterServerConnect(UdpEndPoint endpoint) {
      Raise(new UdpEventMasterServerConnect { EndPoint = endpoint });
    }

    public void MasterServerRequestInfo() {
      Raise(new UdpEventMasterServerRequestInfo());
    }

    public void MasterServerRequestSessionList() {
      Raise(new UdpEventMasterServerRequestSessionList());
    }

    public void LanBroadcastEnable(UdpIPv4Address localAddresss, UdpIPv4Address broadcastAddress, ushort port) {
      Raise(new UdpEventLanBroadcastEnable { LocalAddress = localAddresss, BroadcastAddress = broadcastAddress, Port = port });
    }

    public void LanBroadcastDisable() {
      Raise(new UdpEventLanBroadcastDisable());
    }

    public void SetHostInfo(string name, bool dedicated, byte[] token) {
      Raise(new UdpEventSessionSetHostData { Name = name, Token = token, Dedicated = dedicated });
    }


    internal bool FindChannel(int id, out UdpStreamChannel channel) {
      return streamChannels.TryGetValue(new UdpChannelName(id), out channel);
    }

    internal byte[] GetSendBuffer() {
      Array.Clear(sendBuffer, 0, sendBuffer.Length);
      return sendBuffer;
    }

    internal byte[] GetRecvBuffer() {
      Array.Clear(recvBuffer, 0, recvBuffer.Length);
      return recvBuffer;
    }

    internal uint GetCurrentTime() {
      return platform.GetPrecisionTime();
    }

    internal void Raise(UdpEvent ev) {
      if (ev.IsInternal) {
        lock (eventQueueIn) {
          eventQueueIn.Enqueue(ev);
        }
      }
      else {
        lock (eventQueueOut) {
          eventQueueOut.Enqueue(ev);
        }
      }
    }

    internal bool Send(UdpEndPoint endpoint, byte[] buffer, int length) {
      if (state == UdpSocketState.Running || state == UdpSocketState.Created) {
        return platformSocket.SendTo(buffer, length, endpoint) == length;
      }

      return false;
    }

    internal void SendCommand(UdpEndPoint endpoint, byte cmd) {
      SendCommand(endpoint, cmd, null);
    }

    internal void SendCommand(UdpEndPoint endpoint, byte cmd, byte[] data) {
      int size = 2;

      byte[] buffer = GetSendBuffer();
      buffer[0] = UdpPipe.PIPE_COMMAND;
      buffer[1] = cmd;

      if (data != null) {
        // copy into buffer
        Array.Copy(data, 0, buffer, 2, data.Length);

        // add size
        size += data.Length;
      }

      Send(endpoint, buffer, size);
    }

    bool ChangeState(UdpSocketState from, UdpSocketState to) {
      if (CheckState(from)) {
        state = to;
        return true;
      }

      return false;
    }

    bool CheckState(UdpSocketState s) {
      if (state != s) {
        return false;
      }

      return true;
    }

    void NetworkLoop() {
      bool created = false;
      bool started = false;

      while (state == UdpSocketState.Created || state == UdpSocketState.Running) {
        try {
          if (created == false) {
            UdpLog.Info("socket created");
            created = true;
          }

          while (state == UdpSocketState.Created) {
            ProcessStartEvent();
            Thread.Sleep(1);
          }

          if (started == false) {
            UdpLog.Info("physical socket started");
            started = true;
          }

          while (state == UdpSocketState.Running) {
            uint now = GetCurrentTime();

            RecvDelayedPackets();
            RecvNetworkData();
            ProcessTimeouts();
            ProcessInternalEvents();

            broadcastManager.Update(now);
            sessionManager.Update(now);

            // this does not always exist
            if (masterClient != null) {
              masterClient.Update(now);
            }

            frame += 1;
          }

          UdpLog.Info("socket closed");
          return;
        }
        catch (Exception exn) {
          UdpLog.Error(exn.ToString());
        }
      }
    }

    bool CreatePhysicalSocket(UdpEndPoint ep, UdpSocketState s) {
      UdpLog.Info("Binding physical socket using platform '{0}'", platform.GetType());

      if (ChangeState(UdpSocketState.Created, s)) {
        platformSocket.Bind(ep);

        if (platformSocket.IsBound) {
          UdpLog.Info("Physical socket bound to {0}", platformSocket.EndPoint.ToString());
          return true;
        }
        else {
          ChangeState(s, UdpSocketState.Shutdown);
          UdpLog.Error("Could not bind physical socket, platform error: {0}", platformSocket.Error);
        }
      }
      else {
        UdpLog.Error("Socket has incorrect state: {0}", state);
      }

      return false;
    }

    void AcceptConnection(UdpEndPoint ep, object userToken, byte[] acceptToken, byte[] connectToken) {
      UdpConnection cn = CreateConnection(ep, UdpConnectionMode.Server, connectToken);
      cn.UserToken = userToken;
      cn.AcceptToken = acceptToken;
      cn.ConnectionId = ++connectionIdCounter;

      if (cn.ConnectionId < 2) {
        UdpLog.Error("Incorrect connection id '{0}' assigned to {1}", cn.ConnectionId, ep);
      }

      if (cn.AcceptToken == null) {
        cn.AcceptTokenWithPrefix = BitConverter.GetBytes(cn.ConnectionId);
      }
      else {
        cn.AcceptTokenWithPrefix = new byte[cn.AcceptToken.Length + 4];

        // copy connection id into first 4 bytes of accept token (with prefix)
        Buffer.BlockCopy(BitConverter.GetBytes(cn.ConnectionId), 0, cn.AcceptTokenWithPrefix, 0, 4);

        // copy full accept token into bytes after connection id
        Buffer.BlockCopy(cn.AcceptToken, 0, cn.AcceptTokenWithPrefix, 4, cn.AcceptToken.Length);
      }

      cn.ChangeState(UdpConnectionState.Connected);

      // update
      if (sessionManager != null) {
        sessionManager.SetConnections(connectionLookup.Count, Config.ConnectionLimit);
      }

      if (masterClient != null) {
        // register host with new info
        masterClient.RegisterHost();
      }
    }

    void ProcessTimeouts() {
      if ((frame & 3) == 3) {
        uint now = GetCurrentTime();

        for (int i = 0; i < connectionList.Count; ++i) {
          UdpConnection cn = connectionList[i];

          switch (cn.State) {
            case UdpConnectionState.Connecting:
              cn.ProcessConnectingTimeouts(now);
              break;

            case UdpConnectionState.Connected:
              cn.ProcessConnectedTimeouts(now);
              break;

            case UdpConnectionState.Disconnected:
              cn.ChangeState(UdpConnectionState.Destroy);
              break;

            case UdpConnectionState.Destroy:
              if (DestroyConnection(cn)) {
                --i;
              }
              break;
          }
        }
      }
    }

    void RecvNetworkData() {
      if (platformSocket.RecvPoll(1)) {
        var endpoint = UdpEndPoint.Any;
        var buffer = GetRecvBuffer();
        var bytes = platformSocket.RecvFrom(buffer, ref endpoint);

        if (bytes > 0) {
#if DEBUG
          if (ShouldDropPacket) {
            return;
          }

          if (ShouldDelayPacket) {
            DelayPacket(endpoint, buffer, bytes);
            return;
          }
#endif
          RecvNetworkPacket(endpoint, buffer, bytes);
        }
      }
    }

    void RecvNetworkPacket(UdpEndPoint ep, byte[] buffer, int bytes) {
      switch (buffer[0]) {
        case UdpPipe.PIPE_COMMAND:
          RecvCommand(ep, buffer, bytes);
          break;

        case UdpPipe.PIPE_PACKET:
          RecvPacket(ep, buffer, bytes);
          break;

        case UdpPipe.PIPE_STREAM:
          RecvStream(ep, buffer, bytes);
          break;

        case UdpPipe.PIPE_STREAM_UNRELIABLE:
          RecvStreamUnreliable(ep, buffer, bytes);
          break;

        case Protocol.Message.MESSAGE_HEADER:
          RecvProtocol(ep, buffer, bytes);
          break;
      }
    }

    void RecvStreamUnreliable(UdpEndPoint ep, byte[] buffer, int bytes) {
      UdpConnection cn;

      if (connectionLookup.TryGetValue(ep, out cn)) {
        cn.OnStreamReceived_Unreliable(buffer, bytes);
      }
    }

    void RecvProtocol(UdpEndPoint endpoint, byte[] buffer, int bytes) {
      if (masterClient != null) {
        masterClient.Client.Recv(endpoint, buffer, 0);
      }
    }

    void RecvCommand(UdpEndPoint ep, byte[] buffer, int size) {
      UdpConnection cn;

      if (connectionLookup.TryGetValue(ep, out cn)) {
        cn.OnCommandReceived(buffer, size);
      }
      else {
        RecvConnectionCommand_Unconnected(ep, buffer, size);
      }
    }

    void RecvStream(UdpEndPoint ep, byte[] buffer, int bytes) {
      UdpConnection cn;

      if (connectionLookup.TryGetValue(ep, out cn)) {
        cn.OnStreamReceived(buffer, bytes);
      }
    }

    void RecvPacket(UdpEndPoint ep, byte[] buffer, int size) {
      UdpConnection cn;

      if (connectionLookup.TryGetValue(ep, out cn)) {
        cn.OnPacketReceived(buffer, size);
      }
    }

    void AddPendingConnection(UdpEndPoint endpoint, byte[] token) {
      if (pendingConnections.ContainsKey(endpoint)) {
        return;
      }

      // add to pending list
      pendingConnections.Add(endpoint, token);

      // tell host
      Raise(new UdpEventConnectRequest { EndPoint = endpoint, Token = token });
    }

    UdpConnection CreateConnection(UdpEndPoint endpoint, UdpConnectionMode mode, byte[] connectToken) {
      if (connectionLookup.ContainsKey(endpoint)) {
        UdpLog.Warn("connection for {0} already exists", endpoint);
        return default(UdpConnection);
      }

      UdpConnection cn;

      cn = new UdpConnection(this, mode, endpoint);
      cn.ConnectToken = connectToken;

      connectionLookup.Add(endpoint, cn);
      connectionList.Add(cn);

      return cn;
    }

    bool DestroyConnection(UdpConnection cn) {
      for (int i = 0; i < connectionList.Count; ++i) {
        if (connectionList[i] == cn) {
          connectionList.RemoveAt(i);
          connectionLookup.Remove(cn.RemoteEndPoint);

          cn.Destroy();

          if (mode == UdpSocketMode.Host && sessionManager != null) {
            sessionManager.SetConnections(connectionList.Count, Config.ConnectionLimit);
          }

          return true;
        }
      }

      return false;
    }

    void RecvConnectionCommand_Unconnected(UdpEndPoint endpoint, byte[] buffer, int size) {
      if (buffer[1] == UdpConnection.COMMAND_CONNECT) {
        byte[] connectToken = UdpUtils.ReadToken(buffer, size, 2);

        if (Config.AllowIncommingConnections && ((connectionLookup.Count + pendingConnections.Count) < Config.ConnectionLimit || Config.ConnectionLimit == -1)) {
          if (Config.AutoAcceptIncommingConnections) {
            AcceptConnection(endpoint, null, null, connectToken);
          }
          else {
            AddPendingConnection(endpoint, connectToken);
          }
        }
        else {
          SendCommand(endpoint, UdpConnection.COMMAND_REFUSED);
        }
      }
    }
  }
}
