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
    Physical = 0,
    Cloud = 1
  }

  public partial class UdpSocket {
    /// <summary>
    /// The amount of redundant acks we should do, valid values are:
    /// 8, 16, 24, 32, 40, 48, 56, 64
    /// </summary>
    public static int AckRedundancy {
      // do not change this number unless you know EXACTLY what you are doings
      get { return 64; }
    }

    /// <summary>
    /// If we should calculate network ping or not
    /// </summary>
    public static bool CalculateNetworkPing {
      // do not change this boolean unless you know EXACTLY what you are doings
      get { return true; }
    }

    /// <summary>
    /// The size of the udpkit internal header sent with each packet
    /// </summary>
    public static int HeaderBitSize {
      // do not change this code unless you know EXACTLY what you are doings
      get { return ((UdpHeader.SEQ_BITS + UdpHeader.SEQ_PADD) * 2) + AckRedundancy + (CalculateNetworkPing ? UdpHeader.NETPING_BITS : 0); }
    }

    readonly internal UdpConfig Config;

#if CLOUD
    enum UdpCloudState {
      Disconnected,
      RequestingToken,
      Connecting,
      Connected
    }
#endif

    int handshakeSize = 0;
    byte[] handshakeBuffer = null;
    uint connectionNumberCounter = 1;

    volatile int frame;
    volatile UdpSocketMode mode;
    volatile UdpSocketState state;

#if CLOUD
    Guid cloudToken;
    uint cloudProtocolTime;
    UdpEndPoint cloudEndPoint;
    UdpEndPoint cloudProxyEndPoint;
    UdpEndPoint cloudArbiterEndPoint;
    UdpCloudState cloudState = UdpCloudState.Disconnected;
#endif

#if MASTER
    uint masterProtocolTime;
    UdpEndPoint masterEndPoint = new UdpEndPoint(new UdpIPv4Address(127, 0, 0, 1), 15000);
#endif

    readonly byte[] tempArray;
    readonly Random random;
    readonly UdpStats stats;
    readonly Thread threadSocket;
    readonly UdpPlatform platform;
    readonly UdpStream readStream;
    readonly UdpStream writeStream;
    readonly UdpConfig configCopy;
    readonly UdpStreamPool streamPool;
    readonly AutoResetEvent availableEvent;
    readonly Queue<UdpEvent> eventQueueIn;
    readonly Queue<UdpEvent> eventQueueOut;
    readonly UdpSerializerFactory serializerFactory;
    readonly UdpSessionHandler sessionHandler;
    readonly UdpBroadcastHandler broadcastHandler;
    readonly List<UdpConnection> connList = new List<UdpConnection>();
    readonly UdpSet<UdpEndPoint> pendingConnections = new UdpSet<UdpEndPoint>(new UdpEndPoint.Comparer());
    readonly Dictionary<UdpEndPoint, UdpConnection> connLookup = new Dictionary<UdpEndPoint, UdpConnection>(new UdpEndPoint.Comparer());

    /// <summary>
    /// Local endpoint of this socket
    /// </summary>
    public UdpEndPoint LocalEndPoint {
      get {
#if CLOUD
        if (mode == UdpSocketMode.Cloud) {
          return cloudEndPoint;
        } else 
#endif
        {
          return LocalPhysicalEndPoint;
        }
      }
    }

    /// <summary>
    /// Local endpoint of this socket
    /// </summary>
    public UdpEndPoint LocalPhysicalEndPoint {
      get {
        return platform.EndPoint;
      }
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
    /// Statistics for the entire socket
    /// </summary>
    public UdpStats Statistics {
      get { return stats; }
    }

    /// <summary>
    /// Returns a copy of the active configuration.
    /// Changing values on this copy does nothing.
    /// </summary>
    public UdpConfig ConfigCopy {
      get { return this.configCopy; }
    }

    /// <summary>
    /// The precision time (in ms) of the underlying socket platform
    /// </summary>
    public uint PrecisionTime {
      get { return GetCurrentTime(); }
    }

    public UdpStreamPool StreamPool {
      get { return streamPool; }
    }

    /// <summary>
    /// A thread can wait on this event before calling Poll to make sure at least one event is available
    /// </summary>
    public AutoResetEvent EventsAvailable {
      get { return availableEvent; }
    }

    /// <summary>
    /// A user-assignable object
    /// </summary>
    public object UserToken {
      get;
      set;
    }

    UdpSocket (UdpPlatform platform, UdpSerializerFactory serializerFactory, UdpConfig config) {
      this.platform = platform;
      this.serializerFactory = serializerFactory;
      this.Config = config.Duplicate();
      this.configCopy = config;

      state = UdpSocketState.Created;
      random = new Random();
      stats = new UdpStats();
      availableEvent = new AutoResetEvent(false);
      sessionHandler = new UdpSessionHandler();
      broadcastHandler = new UdpBroadcastHandler(this);

      if (this.Config.NoiseFunction == null) {
        this.Config.NoiseFunction = delegate() { return (float) random.NextDouble(); };
      }

      for (int i = 0; i < config.HandshakeData.Length; ++i) {
        handshakeSize += config.HandshakeData[i].Data.Length;
      }

      handshakeBuffer = new byte[handshakeSize];
      tempArray = new byte[config.PacketSize * 2];
      streamPool = new UdpStreamPool(this);
      readStream = new UdpStream(new byte[config.PacketSize * 2]);
      writeStream = new UdpStream(new byte[config.PacketSize * 2]);

      eventQueueIn = new Queue<UdpEvent>(config.InitialEventQueueSize);
      eventQueueOut = new Queue<UdpEvent>(config.InitialEventQueueSize);

      threadSocket = new Thread(NetworkLoop);
      threadSocket.Name = "udpkit thread";
      threadSocket.IsBackground = true;
      threadSocket.Start();
    }

    /// <summary>
    /// Start this socket
    /// </summary>
    /// <param name="endpoint">The endpoint to bind to</param>
    public void Start (UdpEndPoint endpoint) {
      Raise(UdpEvent.INTERNAL_START, endpoint);
    }

#if CLOUD
    /// <summary>
    /// Start this socket as a cloud socket
    /// </summary>
    /// <param name="endpoint">The physical endpoint to bind to</param>
    public void StartCloud (UdpEndPoint cloud, UdpEndPoint endpoint) {
      Raise(UdpEvent.INTERNAL_CLOUD_SET_MASTER, cloud);
      Raise(UdpEvent.INTERNAL_START_CLOUD, endpoint);
    }
#endif

    /// <summary>
    /// Close this socket
    /// </summary>
    public void Close () {
      Raise(UdpEvent.INTERNAL_CLOSE);
    }

    /// <summary>
    /// Connect to remote endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint to connect to</param>
    public void Connect (UdpEndPoint endpoint) {
      Raise(UdpEvent.INTERNAL_CONNECT, endpoint);
    }

    /// <summary>
    /// Connect to remote endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint to connect to</param>
    public void Connect (UdpEndPoint endpoint, byte[] token) {
      Raise(UdpEvent.INTERNAL_CONNECT, endpoint, token);
    }

    /// <summary>
    /// Cancel ongoing attempt to connect to endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint to cancel connect attempt to</param>
    public void CancelConnect (UdpEndPoint endpoint) {
      Raise(UdpEvent.INTERNAL_CONNECT_CANCEL, endpoint);
    }

    /// <summary>
    /// Removes all existing found sessions
    /// </summary>
    public void ForgetAllSessions () {
      Raise(UdpEvent.INTERNAL_FORGET_ALL_SESSIONS);
    }

    /// <summary>
    /// Accept a connection request from a remote endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint to accept</param>
    public void Accept (UdpEndPoint endpoint) {
      Raise(UdpEvent.INTERNAL_ACCEPT, endpoint);
    }

    /// <summary>
    /// Refuse a connection request from a remote endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint to refuse</param>
    public void Refuse (UdpEndPoint endpoint) {
      Raise(UdpEvent.INTERNAL_REFUSE, endpoint);
    }

    /// <summary>
    /// Suspends the networking thread for N milliseconds. 
    /// Usefull for simulating unusual networking conditions.
    /// </summary>
    /// <param name="milliseconds">How long to sleep</param>
    public void Sleep (int milliseconds) {
#if DEBUG
      Raise(UdpEvent.INTERNAL_SLEEP, milliseconds);
#else
      UdpLog.Warn("Calling UdpSocket.Sleep in non-debug build is not supported");
#endif
    }

    /// <summary>
    /// A list of all currently available sessions
    /// </summary>
    public UdpSession[] GetSessions () {
      return sessionHandler.Sessions.ToArray();
    }

    /// <summary>
    /// Poll socket for any events
    /// </summary>
    /// <param name="ev">The current event on this socket</param>
    /// <returns>True if a new event is available, False otherwise</returns>
    public bool Poll (out UdpEvent ev) {
      lock (eventQueueOut) {
        if (eventQueueOut.Count > 0) {
          ev = eventQueueOut.Dequeue();
          return true;
        }
      }

      ev = default(UdpEvent);
      return false;
    }

    /// <summary>
    /// Peek the next event from the socket
    /// </summary>
    /// <param name="ev">The next event on this socket</param>
    /// <returns>True if an event is available, False otherwise</returns>
    public bool Peek (out UdpEvent ev) {
      lock (eventQueueOut) {
        if (eventQueueOut.Count > 0) {
          ev = eventQueueOut.Peek();
          return true;
        }
      }

      ev = default(UdpEvent);
      return false;
    }

    public void EnableLanBroadcast (UdpEndPoint endpoint, bool isServer) {
      UdpEvent ev = new UdpEvent();
      ev.Type = UdpEvent.INTERNAL_ENABLE_BROADCAST;
      ev.EndPoint = endpoint;
      ev.Object0 = isServer;
      Raise(ev);
    }

    public void DisableLanBroadcast () {
      Raise(UdpEvent.INTERNAL_DISABLE_BROADCAST);
    }

    public void SetSessionData (string serverName, string userData) {
      UdpEvent ev = new UdpEvent();
      ev.Type = UdpEvent.INTERNAL_SET_SESSION_DATA;
      ev.Object0 = serverName;
      ev.Object1 = userData;
      Raise(ev);
    }

    internal void Raise (int eventType) {
      UdpEvent ev = new UdpEvent();
      ev.Type = eventType;
      Raise(ev);
    }

    internal void Raise (int eventType, int intval) {
      UdpEvent ev = new UdpEvent();
      ev.Type = eventType;
      ev.intVal = intval;
      Raise(ev);
    }

    internal void Raise (int eventType, UdpEndPoint endpoint) {
      UdpEvent ev = new UdpEvent();
      ev.Type = eventType;
      ev.EndPoint = endpoint;
      Raise(ev);
    }

    internal void Raise (int eventType, UdpEndPoint endpoint, object object0) {
      UdpEvent ev = new UdpEvent();
      ev.Type = eventType;
      ev.EndPoint = endpoint;
      ev.Object0 = object0;
      Raise(ev);
    }

    internal void Raise (int eventType, UdpEndPoint endpoint, int intVal) {
      UdpEvent ev = new UdpEvent();
      ev.Type = eventType;
      ev.EndPoint = endpoint;
      ev.intVal = intVal;
      Raise(ev);
    }

    internal void Raise (int eventType, UdpConnection connection) {
      UdpEvent ev = new UdpEvent();
      ev.Type = eventType;
      ev.Connection = connection;
      Raise(ev);
    }

    internal void Raise (int eventType, object obj) {
      UdpEvent ev = new UdpEvent();
      ev.Type = eventType;
      ev.Object0 = obj;
      Raise(ev);
    }

    internal void Raise (int eventType, UdpConnection connection, object obj) {
      UdpEvent ev = new UdpEvent();
      ev.Type = eventType;
      ev.Connection = connection;
      ev.Object0 = obj;
      Raise(ev);
    }

    internal void Raise (int eventType, UdpConnection connection, object obj, UdpSendFailReason reason) {
      UdpEvent ev = new UdpEvent();
      ev.Type = eventType;
      ev.Connection = connection;
      ev.FailedReason = reason;
      ev.Object0 = obj;
      Raise(ev);
    }

    internal void Raise (int eventType, UdpConnection connection, UdpConnectionOption option, int value) {
      UdpEvent ev = new UdpEvent();
      ev.Type = eventType;
      ev.Connection = connection;
      ev.Option = option;
      ev.intVal = value;
      Raise(ev);
    }

#if CLOUD
    void SendCloudPacket (ref UdpEndPoint endpoint, byte[] buffer, ref int length) {
      if (endpoint.Address != cloudArbiterEndPoint.Address) {
        // copy all data into temp array
        Buffer.BlockCopy(buffer, 0, tempArray, 0, length);

        // write the target endpoint into the buffer (we dont need a port, the cloud
        // service always uses port 0).
        buffer[0] = 0;
        buffer[1] = endpoint.Address.Byte3;
        buffer[2] = endpoint.Address.Byte2;
        buffer[3] = endpoint.Address.Byte1;
        buffer[4] = endpoint.Address.Byte0;

        // now copy the data from the temp array back into the buffer, but 5 bytes a-head
        Buffer.BlockCopy(tempArray, 0, buffer, 5, length);

        // increment length by five
        length += 5;

        // switch to cloud proxy endpoint
        endpoint = cloudProxyEndPoint;
      }
    }
#endif

    internal bool Send (UdpEndPoint endpoint, byte[] buffer, int length) {
      if (state == UdpSocketState.Running || state == UdpSocketState.Created) {
#if CLOUD
        if (mode == UdpSocketMode.Cloud) {
          SendCloudPacket(ref endpoint, buffer, ref length);
        }
#endif

        int bytes = 0;
        return platform.SendTo(buffer, length, endpoint, ref bytes);
      }

      return false;
    }

    internal float RandomFloat () {
      return (float) random.NextDouble();
    }

    internal UdpSerializer CreateSerializer () {
      return serializerFactory();
    }

    internal UdpStream GetReadStream () {
      // clear data buffer every time
      Array.Clear(readStream.Data, 0, readStream.Data.Length);

      readStream.Ptr = 0;
      readStream.Length = 0;

      return readStream;
    }

    internal UdpStream GetWriteStream () {
      return GetWriteStream(writeStream.Data.Length << 3, 0);
    }

    internal UdpStream GetWriteStream (int length, int offset) {
      // clear data buffer every time
      Array.Clear(writeStream.Data, 0, writeStream.Data.Length);

      writeStream.Ptr = offset;
      writeStream.Length = length;

      return writeStream;
    }

    internal uint GetCurrentTime () {
      return platform.PlatformPrecisionTime;
    }

    internal void Raise (UdpEvent ev) {
      if (ev.IsInternal) {
        lock (eventQueueIn) {
          eventQueueIn.Enqueue(ev);
        }
      } else {
        lock (eventQueueOut) {
          eventQueueOut.Enqueue(ev);
        }

        if (Config.UseAvailableEventEvent) {
          availableEvent.Set();
        }
      }
    }

    void SendRefusedCommand (UdpEndPoint endpoint, UdpHandshakeResult handshake) {
      UdpCommandType type;

      switch (handshake.type) {
        case UdpHandshakeResultType.InvalidSize:
          type = UdpCommandType.Refused_HandshakeSize; break;

        case UdpHandshakeResultType.InvalidValue:
          type = UdpCommandType.Refused_HandshakeValue; break;

        default:
          type = UdpCommandType.Refused; break;
      }

      UdpStream stream = GetWriteStream(Config.PacketSize << 3, HeaderBitSize);
      stream.WriteByte((byte) type, 8);
      stream.WriteInt(handshake.failDataIndex);
      stream.WriteInt(handshake.failBufferLength);

      if (handshake.failBufferLength > 0) {
        UdpAssert.Assert(handshake.type == UdpHandshakeResultType.InvalidValue);
        stream.WriteByteArray(handshakeBuffer, handshake.failBufferOffset, handshake.failBufferLength);
      }

      UdpHeader header = new UdpHeader();
      header.IsObject = false;
      header.AckHistory = 0;
      header.AckSequence = 1;
      header.ObjSequence = 1;
      header.Now = 0;
      header.Pack(stream, this);

      if (Send(endpoint, stream.Data, UdpMath.BytesRequired(stream.Ptr)) == false) {
        // do something here?
      }
    }

    bool ChangeState (UdpSocketState from, UdpSocketState to) {
      if (CheckState(from)) {
        state = to;
        return true;
      }

      return false;
    }

    bool CheckState (UdpSocketState s) {
      if (state != s) {
        return false;
      }

      return true;
    }

    UdpConnection CreateConnection (UdpEndPoint endpoint, UdpConnectionMode mode) {
      if (connLookup.ContainsKey(endpoint)) {
        UdpLog.Warn("connection for {0} already exists", endpoint);
        return default(UdpConnection);
      }

      UdpConnection cn;
      cn = new UdpConnection(this, mode, endpoint);

      if (mode == UdpConnectionMode.Server) {
        cn.id = ++connectionNumberCounter;
        UdpLog.Debug("created connection with id {0}", cn.id);
      }

      connLookup.Add(endpoint, cn);
      connList.Add(cn);

      return cn;
    }

    bool DestroyConnection (UdpConnection cn) {
      for (int i = 0; i < connList.Count; ++i) {
        if (connList[i] == cn) {
          connList.RemoveAt(i);
          connLookup.Remove(cn.RemoteEndPoint);

          cn.Destroy();

          return true;
        }
      }

      return false;
    }

    void NetworkLoop () {
      bool created = false;
      bool started = false;

#if CLOUD
      bool virtualStarted = false;
#endif

      while (state == UdpSocketState.Created || state == UdpSocketState.Running) {
#if DEBUG
        try {
#endif
          if (created == false) {
            UdpLog.Info("socket created");
            created = true;
          }

          while (state == UdpSocketState.Created) {
            ProcessIncommingEvents(true);
            Thread.Sleep(1);
          }

          if (started == false) {
            UdpLog.Info("physical socket started");
            started = true;
          }

#if CLOUD
        if (mode == UdpSocketMode.Cloud) {
          while (cloudState == UdpCloudState.RequestingToken || cloudState == UdpCloudState.Connecting) {
            RecvDelayedPackets();
            RecvNetworkData();
            PerformCloudHandshake();
            Thread.Sleep(1);
          }

          if (virtualStarted == false) {
            UdpLog.Info("virtual socket started");
            virtualStarted = true;
          }
        }
#endif

          while (state == UdpSocketState.Running) {
            RecvDelayedPackets();
            RecvNetworkData();
            ProcessTimeouts();
            ProcessSessionDiscovery();
            ProcessIncommingEvents(false);

            frame += 1;
          }

          UdpLog.Info("socket closed");
          return;

#if DEBUG
        } catch (Exception exn) {
          UdpLog.Error(exn.ToString());
        }
#endif
      }
    }

    void ProcessSessionDiscovery () {
      uint now = GetCurrentTime();

      if (platform.IsBroadcasting) {
        broadcastHandler.PackData(now);
        broadcastHandler.ReadData(now);
      }

      sessionHandler.RemoveOldSessions(now);
    }

#if CLOUD
    void PerformCloudHandshake () {
      uint now = GetCurrentTime();

      switch (cloudState) {
        case UdpCloudState.RequestingToken: RequestCloudToken(now); break;
        case UdpCloudState.Connecting: RequestCloudConnect(now); break;
      }
    }

    void RequestCloudConnect (uint now) {
      const int HEADER_CONNECT_REQUEST = 5;

      if ((cloudProtocolTime + 1000) < now) {
        try {
          UdpStream stream = GetWriteStream();
          stream.WriteByte(HEADER_CONNECT_REQUEST);
          stream.WriteByteArray(cloudToken.ToByteArray());

          Send(cloudProxyEndPoint, stream.Data, UdpMath.BytesRequired(stream.Ptr));

        } finally {
          cloudProtocolTime = now;
        }
      }
    }

    void RequestCloudToken (uint now) {
      const int HEADER_REQUEST_TOKEN = 1;

      if ((cloudProtocolTime + 1000) < now) {
        try {
          UdpStream stream = GetWriteStream();
          stream.WriteByte(HEADER_REQUEST_TOKEN);

          Send(cloudArbiterEndPoint, stream.Data, UdpMath.BytesRequired(stream.Ptr));

        } finally {
          cloudProtocolTime = now;
        }
      }
    }
#endif

    void ProcessIncommingEvents (bool returnOnStart) {
      while (true) {
        UdpEvent ev = default(UdpEvent);

        lock (eventQueueIn) {
          if (eventQueueIn.Count > 0) {
            ev = eventQueueIn.Dequeue();
          }
        }

        if (ev.Type == 0) {
          return;
        }

        switch (ev.Type) {
          case UdpEvent.INTERNAL_START:
            OnEventStart(ev);
            if (returnOnStart) { return; } else { break; }

#if CLOUD
          case UdpEvent.INTERNAL_START_CLOUD:
            OnEventStartVirtual(ev);
            if (returnOnStart) { return; } else { break; }
#endif

          case UdpEvent.INTERNAL_CONNECT: OnEventConnect(ev); break;
          case UdpEvent.INTERNAL_CONNECT_CANCEL: OnEventConnectCancel(ev); break;
          case UdpEvent.INTERNAL_ACCEPT: OnEventAccept(ev); break;
          case UdpEvent.INTERNAL_REFUSE: OnEventRefuse(ev); break;
          case UdpEvent.INTERNAL_DISCONNECT: OnEventDisconect(ev); break;

          case UdpEvent.INTERNAL_CLOSE:
            OnEventClose(ev);
            return;
            break;

          case UdpEvent.INTERNAL_SEND: OnEventSend(ev); break;
          case UdpEvent.INTERNAL_CONNECTION_OPTION: OnEventConnectionOption(ev); break;
          case UdpEvent.INTERNAL_SLEEP: OnEventSleep(ev); break;
          case UdpEvent.INTERNAL_ENABLE_BROADCAST: OnEventEnableBroadcast(ev); break;
          case UdpEvent.INTERNAL_DISABLE_BROADCAST: OnEventDisableBroadcast(ev); break;
          case UdpEvent.INTERNAL_SET_SESSION_DATA: OnEventSetSessionData(ev); break;
          case UdpEvent.INTERNAL_FORGET_ALL_SESSIONS: OnEventForgetAllSessions(ev); break;
#if CLOUD
          case UdpEvent.INTERNAL_CLOUD_SET_MASTER: OnEventSetCloudMaster(ev); break;
#endif
        }
      }
    }

    void OnEventForgetAllSessions (UdpEvent ev) {
      sessionHandler.Sessions = new UdpBag<UdpSession>();
    }

    void OnEventSetSessionData (UdpEvent ev) {
      sessionHandler.Name = (string) ev.Object0;
      sessionHandler.Data = (string) ev.Object1;
    }

#if CLOUD
    void OnEventSetCloudMaster (UdpEvent ev) {
      UdpLog.Info("set cloud master to {0}", ev);
      cloudArbiterEndPoint = ev.EndPoint;
    }
#endif

    void OnEventDisableBroadcast (UdpEvent ev) {
      broadcastHandler.Disable();
    }

    void OnEventEnableBroadcast (UdpEvent ev) {
      broadcastHandler.Enable(ev.EndPoint, (bool) ev.Object0);
    }

#if CLOUD
    void OnEventStartVirtual (UdpEvent ev) {
      mode = UdpSocketMode.Cloud;

      if (CreatePhysicalSocket(ev.EndPoint, UdpSocketState.Running)) {
        cloudState = UdpCloudState.RequestingToken;
      } else {
        Raise(UdpEvent.PUBLIC_START_FAILED);
      }
    }
#endif

    void OnEventStart (UdpEvent ev) {
      mode = UdpSocketMode.Physical;

      if (CreatePhysicalSocket(ev.EndPoint, UdpSocketState.Running)) {
        Raise(UdpEvent.PUBLIC_STARTED, platform.EndPoint);
      } else {
        Raise(UdpEvent.PUBLIC_START_FAILED);
      }
    }

    bool CreatePhysicalSocket (UdpEndPoint ep, UdpSocketState s) {
      UdpLog.Info("binding physical socket using platform '{0}'", platform.GetType());

      if (ChangeState(UdpSocketState.Created, s)) {
        if (platform.Bind(ep)) {
          UdpLog.Info("physical socket bound to {0}", platform.EndPoint.ToString());
          return true;
        } else {
          UdpLog.Error("could not bind physical socket, platform error: {0}", platform.PlatformErrorString);
        }
      } else {
        UdpLog.Error("socket has in correct state: {0}", state);
      }

      return false;
    }

    void OnEventConnect (UdpEvent ev) {
      if (CheckState(UdpSocketState.Running)) {
        // always stop broadcasting if we join someone
        OnEventDisableBroadcast(default(UdpEvent));

        // start joining
        UdpConnection cn = CreateConnection(ev.EndPoint, UdpConnectionMode.Client);
        cn.token = ev.Object0 as byte[];

        if (cn == null) {
          UdpLog.Error("could not create connection for endpoint {0}", ev.EndPoint.ToString());
        } else {
          UdpLog.Info("connecting to {0}", ev.EndPoint.ToString());
        }
      }
    }

    void OnEventConnectCancel (UdpEvent ev) {
      if (CheckState(UdpSocketState.Running)) {
        UdpConnection cn;

        if (connLookup.TryGetValue(ev.EndPoint, out cn)) {
          // if we are connecting, destroy connection
          if (cn.CheckState(UdpConnectionState.Connecting)) {
            // notify user thread
            Raise(UdpEvent.PUBLIC_CONNECT_FAILED, ev.EndPoint);

            // destroy this connection
            cn.ChangeState(UdpConnectionState.Destroy);
          }

          // if we are connected, disconnect 
          else if (ev.Connection.CheckState(UdpConnectionState.Connected)) {
            ev.Connection.SendCommand(UdpCommandType.Disconnected);
            ev.Connection.ChangeState(UdpConnectionState.Disconnected);
          }
        }
      }
    }

    void OnEventAccept (UdpEvent ev) {
      if (pendingConnections.Remove(ev.EndPoint)) {
        AcceptConnection(ev.EndPoint);
      }
    }

    void OnEventRefuse (UdpEvent ev) {
      if (pendingConnections.Remove(ev.EndPoint)) {
        SendRefusedCommand(ev.EndPoint, new UdpHandshakeResult());
      }
    }

    void OnEventDisconect (UdpEvent ev) {
      if (ev.Connection.CheckState(UdpConnectionState.Connected)) {
        ev.Connection.SendCommand(UdpCommandType.Disconnected);
        ev.Connection.ChangeState(UdpConnectionState.Disconnected);
      }
    }

    void OnEventClose (UdpEvent ev) {
      if (ChangeState(UdpSocketState.Running, UdpSocketState.Shutdown)) {
        for (int i = 0; i < connList.Count; ++i) {
          UdpConnection cn = connList[i];
          cn.SendCommand(UdpCommandType.Disconnected);
          cn.ChangeState(UdpConnectionState.Disconnected);
        }

        if (platform.Close() == false) {
          UdpLog.Error("failed to shutdown socket interface, platform error: {0}", platform.PlatformErrorString);
        }

        connList.Clear();
        connLookup.Clear();
        eventQueueIn.Clear();
        pendingConnections.Clear();

        GetReadStream().Data = null;
        GetWriteStream(0, 0).Data = null;
      }
    }

    void OnEventSend (UdpEvent ev) {
      ev.Connection.SendObject(ev.Object0);
    }

    void OnEventSleep (UdpEvent ev) {
      UdpLog.Debug("sleeping network thread for {0} ms", ev.intVal);
      Thread.Sleep(ev.intVal);
    }

    void OnEventConnectionOption (UdpEvent ev) {
      ev.Connection.OnEventConnectionOption(ev);
    }

    void AcceptConnection (UdpEndPoint ep) {
      UdpConnection cn = CreateConnection(ep, UdpConnectionMode.Server);
      cn.ChangeState(UdpConnectionState.Connected);
    }

    void ProcessTimeouts () {
      if ((frame & 3) == 3) {
        uint now = GetCurrentTime();

        for (int i = 0; i < connList.Count; ++i) {
          UdpConnection cn = connList[i];

          switch (cn.state) {
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

#if CLOUD
    void RecvCloudPacket (ref UdpEndPoint ep, UdpStream stream) {
      // make sure this is from the cloud service
      if (ep.Address != cloudArbiterEndPoint.Address) {
        UdpLog.Warn("received cloud packet from {0}, which does not have the same IP as the cloud master {1} ", ep.Address, cloudArbiterEndPoint.Address);
        return;
      }

      // data packet
      if (stream.Data[0] == 0) {
        UdpIPv4Address cloudip = 
          new UdpIPv4Address(
            stream.Data[1],
            stream.Data[2],
            stream.Data[3],
            stream.Data[4]);

        // replace endpoint with our "special" cloud endpoint
        ep = new UdpEndPoint(cloudip, 0);

        // copy all data from the stream to our temp array
        Buffer.BlockCopy(stream.Data, 0, tempArray, 0, stream.Data.Length);

        // copy back all the data except the first five bytes (header byte + cloud ip)
        Buffer.BlockCopy(tempArray, 5, stream.Data, 0, stream.Data.Length - 5);

        // update stream length (40 bits less, 5 bytes at 8 bits each)
        stream.Length = stream.Length - (5 * 8);
      }
    }
#endif

    void RecvNetworkData () {
      if (platform.RecvPoll(1)) {
        int bytes = 0;
        UdpEndPoint ep = UdpEndPoint.Any;
        UdpStream stream = GetReadStream();

        if (platform.RecvFrom(stream.Data, stream.Data.Length, ref bytes, ref ep)) {
#if CLOUD
          if (mode == UdpSocketMode.Cloud) {
            RecvCloudPacket(ref ep, stream);
          }
#endif

#if DEBUG
          if (ShouldDropPacket) {
            return;
          }

          if (ShouldDelayPacket) {
            DelayPacket(ep, stream.Data, bytes);
            return;
          }
#endif

          RecvNetworkPacket(ep, stream, bytes);
        }
      }
    }

    void RecvNetworkPacket (UdpEndPoint ep, UdpStream stream, int bytes) {
      // set stream length
      stream.Length = bytes << 3;

      // try to grab connection
      UdpConnection cn;

      if (connLookup.TryGetValue(ep, out cn)) {
        // deliver to connection
        cn.OnPacket(stream);

      } else {
        // handle unconnected data
        RecvUnconnectedPacket(stream, ep);
      }
    }

    void RecvUnconnectedPacket (UdpStream buffer, UdpEndPoint ep) {
#if CLOUD
      const byte TOKEN_GRANTED = 3;
      const byte CONNECTION_GRANTED = 7;

      if (mode == UdpSocketMode.Cloud && ep.Address == cloudArbiterEndPoint.Address) {
        byte header = buffer.ReadByte();

        switch (header) {
          case TOKEN_GRANTED: CloudTokenGranted(buffer, ep); break;
          case CONNECTION_GRANTED: CloudConnectionGranted(buffer, ep); break;

          default:
            UdpLog.Warn("received invalid header byte ({0}) from cloud server at {1}", header, ep);
            break;
        }
      } else 
#endif
      {
        UdpAssert.Assert(buffer.Ptr == 0);
        buffer.Ptr = HeaderBitSize;

        if (buffer.ReadByte(8) == (byte) UdpCommandType.Connect) {

          byte[] token = null;

          if (buffer.ReadBool()) {
            int length = buffer.ReadInt();

            if ((length < 0) || (length > 256)) {
              SendRefusedCommand(ep, new UdpHandshakeResult());
              return;
            }

            token = new byte[length];
            buffer.ReadByteArray(token);  
          }

          UdpHandshakeResult handshake = VerifyHandshake(buffer);

          switch (handshake.type) {
            case UdpHandshakeResultType.Success:
              if (Config.AllowIncommingConnections && ((connLookup.Count + pendingConnections.Count) < Config.ConnectionLimit || Config.ConnectionLimit == -1)) {
                if (Config.AutoAcceptIncommingConnections) {
                  AcceptConnection(ep);
                } else {
                  if (pendingConnections.Add(ep)) {
                    Raise(UdpEvent.PUBLIC_CONNECT_REQUEST, ep, token);
                  }
                }
              } else {
                SendRefusedCommand(ep, new UdpHandshakeResult());
              }
              break;

            case UdpHandshakeResultType.InvalidSize:
              SendRefusedCommand(ep, handshake);
              break;

            case UdpHandshakeResultType.InvalidValue:
              SendRefusedCommand(ep, handshake);
              break;
          }
        }
      }
    }

    UdpHandshakeResult VerifyHandshake (UdpStream buffer) {
      UdpLog.Info("performing handshake ...");

      UdpHandshakeResult result = new UdpHandshakeResult();
      UdpAssert.Assert(handshakeBuffer.Length == handshakeSize);

      if (handshakeSize == 0 && buffer.Done) {
        UdpLog.Info("handshake done (none)");
        result.type = UdpHandshakeResultType.Success;
        return result;
      }

      buffer.ReadByteArray(handshakeBuffer, 0, handshakeSize);

      if (buffer.Overflowing) {
        UdpLog.Info("handshake failed (size - overflow)");
        result.type = UdpHandshakeResultType.InvalidSize;
        return result;
      }

      int handshakeBufferOffset = 0;

      for (int i = 0; i < Config.HandshakeData.Length; ++i) {
        int startBufferOffset = handshakeBufferOffset;

        for (int k = 0; k < Config.HandshakeData[i].Data.Length; ++k) {
          if (Config.HandshakeData[i].Data[k] != handshakeBuffer[handshakeBufferOffset]) {
            UdpLog.Info("handshake failed (value - {0})", Config.HandshakeData[i].Name);
            result.type = UdpHandshakeResultType.InvalidValue;
            result.failDataIndex = i;
            result.failBufferOffset = startBufferOffset;
            result.failBufferLength = Config.HandshakeData[i].Data.Length;
            return result;
          }

          ++handshakeBufferOffset;
        }
      }

      UdpLog.Info("handshake success (value)");
      result.type = UdpHandshakeResultType.Success;
      return result;
    }

#if CLOUD
    void CloudConnectionGranted (UdpStream buffer, UdpEndPoint ep) {
      if (ep != cloudProxyEndPoint) {
        UdpLog.Warn("received connection granted packet from invalid cloud endpoint {0}, expected endpoint: {1}", ep, cloudProxyEndPoint);
        return;
      }

      if (cloudState != UdpCloudState.Connecting) {
        UdpLog.Warn("received connection granted packet from {0} when state was {1}", ep, cloudState);
        return;
      }

      // yay!
      UdpLog.Info("connected to cloud, with virtual endpoint {0}", cloudEndPoint);

      // we are connected
      cloudState = UdpCloudState.Connected;

      // started!
      Raise(UdpEvent.PUBLIC_STARTED, cloudEndPoint);
    }

    void CloudTokenGranted (UdpStream buffer, UdpEndPoint ep) {
      if (cloudState != UdpCloudState.RequestingToken) {
        return;
      }

      byte a = buffer.ReadByte();
      byte b = buffer.ReadByte();
      byte c = buffer.ReadByte();
      byte d = buffer.ReadByte();

      cloudEndPoint = new UdpEndPoint(new UdpIPv4Address(a, b, c, d), 0);

      byte[] guid = new byte[16];
      buffer.ReadByteArray(guid);

      cloudToken = new Guid(guid);
      cloudProxyEndPoint = new UdpEndPoint(cloudArbiterEndPoint.Address, buffer.ReadUShort());
      cloudState = UdpCloudState.Connecting;
    }
#endif

    public static UdpSocket Create (UdpPlatform platform, UdpSerializerFactory serializer, UdpConfig config) {
      return new UdpSocket(platform, serializer, config);
    }

    public static UdpSocket Create (UdpPlatform platform, UdpSerializerFactory serializer) {
      return Create(platform, serializer, new UdpConfig());
    }

    public static UdpSocket Create<TPlatform, TSerializer> (UdpConfig config)
      where TPlatform : UdpPlatform, new()
      where TSerializer : UdpSerializer, new() {
      return new UdpSocket(new TPlatform(), () => new TSerializer(), config);
    }

    public static UdpSocket Create<TPlatform, TSerializer> ()
      where TPlatform : UdpPlatform, new()
      where TSerializer : UdpSerializer, new() {
      return Create<TPlatform, TSerializer>(new UdpConfig());
    }

    public static UdpSocket Create (UdpPlatform platform, UdpConfig config) {
      return Create(platform, () => new UdpStreamSerializer(), config);
    }

    public static UdpSocket Create (UdpPlatform platform) {
      return Create(platform, new UdpConfig());
    }

    public static UdpSocket Create<TPlatform> ()
        where TPlatform : UdpPlatform, new() {
      return Create(new TPlatform(), new UdpConfig());
    }

    public static UdpSocket Create<TPlatform> (UdpConfig config)
        where TPlatform : UdpPlatform, new() {
      return Create(new TPlatform(), config);
    }

    public static UdpSocketMultiplexer CreateMultiplexer (params UdpSocket[] sockets) {
      return new UdpSocketMultiplexer(sockets);
    }

    public static UdpSocketMultiplexer CreateMultiplexer<TPlatform, TSerializer> (UdpIPv4Address address, ushort portMin, ushort portMax)
      where TPlatform : UdpPlatform, new()
      where TSerializer : UdpSerializer, new() {
      return CreateMultiplexer<TPlatform, TSerializer>(address, portMin, portMax, new UdpConfig());
    }

    public static UdpSocketMultiplexer CreateMultiplexer<TPlatform, TSerializer> (UdpIPv4Address address, ushort portMin, ushort portMax, UdpConfig config)
      where TPlatform : UdpPlatform, new()
      where TSerializer : UdpSerializer, new() {

      if (portMin > portMax) {
        throw new ArgumentOutOfRangeException("portMin was larger then portMax");
      }

      List<UdpSocket> sockets = new List<UdpSocket>();

      for (; portMin <= portMax; portMin += 1) {
        // create and start socket
        UdpSocket s = Create<TPlatform, TSerializer>(config);
        s.Start(new UdpEndPoint(address, portMin));

        // add to list
        sockets.Add(s);
      }

      return CreateMultiplexer(sockets.ToArray());
    }
  }
}
