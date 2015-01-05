using Bolt;
using System;
using UdpKit;
using UnityEngine;

[Documentation(Ignore = true)]
public struct PacketStats {
  public int StateBits;
  public int EventBits;
  public int CommandBits;
}

/// <summary>
/// The connection to a remote endpoint
/// </summary>
/// <example>
/// *Example:* Accepting an incoming connection.
/// 
/// ```csharp
/// public override void ConnectRequest(UdpEndPoint endpoint) {
///   BoltNetwork.Accept(endPoint);
/// }
/// ```
/// </example>
[DocumentationAttribute]
public class BoltConnection : BoltObject {

  UdpConnection _udp;
  BoltChannel[] _channels;

  int _framesToStep;
  int _packetsReceived;
  int _packetCounter;

  int _remoteFrameDiff;
  int _remoteFrameActual;
  int _remoteFrameEstimated;
  bool _remoteFrameAdjust;

  int _bitsSecondIn;
  int _bitsSecondInAcc;

  int _bitsSecondOut;
  int _bitsSecondOutAcc;

  internal EventChannel _eventChannel;
  internal SceneLoadChannel _sceneLoadChannel;
  internal EntityChannel _entityChannel;
  internal EntityChannel.CommandChannel _commandChannel;

  internal BoltRingBuffer<PacketStats> _packetStatsIn;
  internal BoltRingBuffer<PacketStats> _packetStatsOut;

  internal bool _canReceiveEntities = true;
  internal SceneLoadState _remoteSceneLoading;

  /// <summary>
  /// Returns true if the remote computer on the other end of this connection is loading a map currently, otherwise false
  /// </summary>
  /// <example>
  /// *Example:* Removing a preloaded player entity from the game if they disconnect while loading.
  /// 
  /// ```csharp
  /// public override void Disconnected(BoltConnection connection) {
  ///   if(connection.isLoadingMap) {
  ///     PlayerEntityList.instance.RemoveFor(connection);
  ///   }
  /// }
  /// ```
  /// </example>
  public bool IsLoadingMap {
    get {
      return
        (_remoteSceneLoading.Scene != BoltCore._localSceneLoading.Scene) ||
        (_remoteSceneLoading.State != SceneLoadState.STATE_CALLBACK_INVOKED);
    }
  }

  [Obsolete("Use BoltConnection.IsLoadingMap instead")]
  public bool isLoadingMap {
    get { return IsLoadingMap; }
  }

  /// <summary>
  /// The estimated frame of the simulation running at the other end of this connection
  /// </summary>
  /// <example>
  /// *Example:* Calculating the average frame difference of the client and server for all clients.
  /// 
  /// ```csharp
  /// float EstimateFrameDiff() {
  ///   int count
  ///   float avg;
  ///   
  ///   foreach(BoltConnection client in BoltNetwork.clients) {
  ///     count++;
  ///     avg += BoltNetwork.serverFrame - client.remoteFrame;
  ///   }
  ///   avg = avg / count;
  ///   return avg;
  /// }
  /// ```
  /// </example>
  public int RemoteFrame {
    get { return _remoteFrameEstimated; }
  }

  [Obsolete("Use BoltConnection.PingNetwork instead")]
  public int remoteFrame {
    get { return RemoteFrame; }
  }

  /// <summary>
  /// A data token that was passed by the client when initiating a connection
  /// </summary>
  /// <example>
  /// *Example:* Using the ```UserInfo``` token to get a fingerprint that identifies the client's local computer.
  /// 
  /// ```csharp
  /// void StorePlayerScore(BoltConnection connection, ScoreData score) {
  ///   Guid fingerprint;  
  /// 
  ///   UserInfo userInfo = (UserInfo)ConnectToken;
  ///   if(userInfo.fingerprint != null) {
  ///     fingerprint = userInfo.fingerprint;  
  ///   }
  ///   else {
  ///     fingerprint = Guid.NewGuid();
  ///     userInfo.fingerprint = fingerprint;
  ///   }
  ///   
  ///   database.PersistScore(userInfo.name, fingerprint, score);
  /// }
  /// ```
  /// </example>
  public IProtocolToken ConnectToken {
    get;
    internal set;
  }

  /// <summary>
  /// A data token that was passed by the server when accepting the connection
  /// </summary>
  /// <example>
  /// *Example:* Using the ```AcceptToken``` to store connection settings.
  /// 
  /// ```csharp
  /// public override void Disconnected(BoltConnection connection, IProtocolToken token) {
  ///   ConnectionSettings connSettings = (ConnectionSettings)token;
  ///   
  ///   StartCoroutine(RemoveIfTimeout(connection, connSettings.maxTimeout));
  /// }
  /// ```
  /// </example>
  public IProtocolToken AcceptToken {
    get;
    internal set;
  }

  [Obsolete("Use BoltConnection.PingNetwork instead")]
  public float ping {
    get { return _udp.NetworkPing; }
  }

  /// <summary>
  /// The round-trip time on the network
  /// </summary>
  /// <example>
  /// *Example:* Displaying the network ping when in debug mode.
  /// 
  /// ```csharp
  /// void OnGUI() {
  ///   if(BoltNetwork.isConnected && BoltNetwork.isClient) {
  ///     GUILayout.Label("Ping:" + BoltNetwork.server.PingNetwork;
  ///   }
  /// }
  /// ```
  /// </example>
  public float PingNetwork {
    get { return _udp.NetworkPing; }
  }

  [Obsolete("Use BoltConnection.PingNetwork instead")]
  public float pingNetwork {
    get { return PingNetwork; }
  }

  /// <summary>
  /// The dejitter delay in number of frames
  /// </summary>
  /// <example>
  /// *Example:* Showing the dejitter delay frames and ping.
  /// 
  /// ```csharp
  /// void OnGUI() {
  ///   if(BoltNetwork.isConnected && BoltNetwork.isClient) {
  ///     GUILayout.Label("Ping:" + BoltNetwork.server.pingNetwork;
  ///     GUILayout.Label("Dejitter Delay:" + BoltNetwork.server.DejitterFrames;
  ///   }
  /// }
  /// ```
  /// </example>
  public int DejitterFrames {
    get { return _remoteFrameActual - _remoteFrameEstimated; }
  }

  [Obsolete("Use BoltConnection.DejitterFrames instead")]
  public int dejitterFrames {
    get { return DejitterFrames; }
  }

  /// <summary>
  /// The round-trip time across the network, including processing delays and acks
  /// </summary>
  /// <example>
  /// *Example:* Showing the difference between ping and aliased ping. Aliased ping will always be larger.
  /// 
  /// ```csharp
  /// void OnGUI() {
  ///   if(BoltNetwork.isConnected && BoltNetwork.isClient) {
  ///     GUILayout.Label("Ping:" + BoltNetwork.server.PingNetwork;
  ///     GUILayout.Label("Ping (Aliased):" + BoltNetwork.server.PingAliased;
  ///   }
  /// }
  /// ```
  /// </example>
  public float PingAliased {
    get { return _udp.NetworkPing; }
  }

  [Obsolete("Use BoltConnection.PingAliased instead")]
  public float pingAliased {
    get { return _udp.AliasedPing; }
  }

  internal UdpConnection udpConnection {
    get { return _udp; }
  }

  internal int remoteFrameLatest {
    get { return _remoteFrameActual; }
  }

  internal int remoteFrameDiff {
    get { return _remoteFrameDiff; }
  }

  /// <summary>
  /// How many bits per second we are receiving in
  /// </summary>
  /// <example>
  /// *Example:* Showing the ping and data flow in and out.
  /// 
  /// ```csharp
  /// void OnGUI() {
  ///   if(BoltNetwork.isConnected && BoltNetwork.isClient) {
  ///     GUILayout.Label("Ping:" + BoltNetwork.server.PingNetwork;
  ///     GUILayout.Label("Bandwidth Out:" + BoltNetwork.server.BitsPerSecondOut);
  ///     GUILayout.Label("Bandwidth In:" + BoltNetwork.server.BitsPerSecondIn);
  ///   }
  /// }
  /// ```
  /// </example>
  public int BitsPerSecondIn {
    get { return _bitsSecondIn; }
  }

  [Obsolete("Use BoltConnection.BitsPerSecondIn instead")]
  public int bitsPerSecondIn {
    get { return BitsPerSecondIn; }
  }

  /// <summary>
  /// How many bits per second we are sending out
  /// </summary>
  /// <example>
  /// *Example:* Showing the ping and data flow in and out.
  /// 
  /// ```csharp
  /// void OnGUI() {
  ///   if(BoltNetwork.isConnected && BoltNetwork.isClient) {
  ///     GUILayout.Label("Ping:" + BoltNetwork.server.pingNetwork;
  ///     GUILayout.Label("Bandwidth Out:" + BoltNetwork.server.bitsPerSecondIn);
  ///     GUILayout.Label("Bandwidth In:" + BoltNetwork.server.bitsPerSecondOut);
  ///   }
  /// }
  /// ```
  /// </example>
  public int BitsPerSecondOut {
    get { return _bitsSecondOut; }
  }

  [Obsolete("Use BoltConnection.BitsPerSecondOut instead")]
  public int bitsPerSecondOut {
    get { return BitsPerSecondOut; }
  }

  public uint ConnectionId {
    get { return udpConnection.ConnectionId; }
  }

  /// <summary>
  /// Remote end point of this connection
  /// </summary>
  /// <example>
  /// *Example:* Logging the address of new connections
  /// 
  /// ```csharp
  /// public override void Connected(BoltConnection connection) {
  ///   ServerLog.Write(string.Format("[{0}:{1}] New Connection", connection.remoteEndPoint.Address, connection.remoteEndPoint.Port);
  /// }
  /// ```
  /// </example>
  public UdpEndPoint RemoteEndPoint {
    get { return udpConnection.RemoteEndPoint; }
  }

  [Obsolete("Use BoltConnection.RemoteEndPoint instead")]
  public UdpEndPoint remoteEndPoint {
    get { return RemoteEndPoint; }
  }

  /// <summary>
  /// User assignable object which lets you pair arbitrary data with the connection
  /// </summary>
  /// <example>
  /// *Example:* Using a reference to the player entity in the UserData property.
  /// 
  /// ```csharp
  /// public override void Disconnected(BoltConnection connection) {
  ///   BoltNetwork.Destroy((BoltEntity)connection.UserData);
  /// }
  /// ```
  /// </example>
  public object UserData {
    get;
    set;
  }

  [Obsolete("Use the 'UserData' property instead")]
  public object userToken {
    get { return UserData; }
    set { UserData = value; }
  }

  internal int SendRateMultiplier {
    get {
      float r = udpConnection.WindowFillRatio;

      if (r < 0.25f) {
        return 1;
      }

      r = r - 0.25f;
      r = r / 0.75f;

      return Mathf.Clamp((int)(r * 60), 1, 60);
    }
  }

  internal BoltConnection(UdpConnection udp) {
    UserData = udp.UserToken;

    _udp = udp;
    _udp.UserToken = this;

    _channels = new BoltChannel[] {
      _sceneLoadChannel = new SceneLoadChannel(),
      _eventChannel = new EventChannel(),
      _commandChannel = new EntityChannel.CommandChannel(),
      _entityChannel = new EntityChannel(),
    };

    _remoteFrameAdjust = false;
    _remoteSceneLoading = SceneLoadState.DefaultRemote();

    _packetStatsOut = new BoltRingBuffer<PacketStats>(BoltCore._config.framesPerSecond);
    _packetStatsOut.autofree = true;

    _packetStatsIn = new BoltRingBuffer<PacketStats>(BoltCore._config.framesPerSecond);
    _packetStatsIn.autofree = true;

    // set channels connection
    for (int i = 0; i < _channels.Length; ++i) {
      _channels[i].connection = this;
    }
  }

  /// <summary>
  /// Send a binary stream of data to this connection
  /// </summary>
  /// <param name="channel">The channel to send on</param>
  /// <param name="data">The binary data</param>
  /// <example>
  /// *Example:* Sending the binary data of a custom icon texture to the server using a static reference 
  /// to the "PlayerIcon" channel that was created inside a ```Channels``` class.
  /// 
  /// ```csharp
  /// void SendCustomIcon(Texture2D myCustomIcon) {
  ///   byte[] data = myCustomIcon.EncodeToPNG();
  ///   
  ///   BoltNetwork.server.StreamBytes(Channels.PlayerIcon, data);
  /// }
  /// ```
  /// </example>
  public void StreamBytes(UdpChannelName channel, byte[] data) {
    _udp.StreamBytes(channel, data);
  }

  /// <summary>
  /// Set the max amount of data allowed per second
  /// </summary>
  /// <param name="bytesPerSecond">The rate in bytes / sec</param>
  /// <example>
  /// *Example:* Configuring the initial stream bandwidth of new connections to 20 kb/s.
  /// 
  /// ```csharp
  /// public override void Connected(BoltConnection connection) {
  ///   connection.SetStreamBandwidth(1024 * 20);
  /// }
  /// ```
  /// </example>
  public void SetStreamBandwidth(int bytesPerSecond) {
    _udp.StreamSetBandwidth(bytesPerSecond);
  }

  /// <summary>
  /// Disconnect this connection
  /// </summary>
  /// <example>
  /// *Example:* Terminating all connections.
  /// 
  /// ```csharp
  /// void DisconnectAll() {
  ///   foreach(var connection in BoltNetwork.connections) {
  ///     connection.Disconnect();
  ///   }
  /// }
  /// ```
  /// </example>
  public void Disconnect() {
    Disconnect(null);
  }

  /// <summary>
  /// Disconnect this connection with custom data
  /// </summary>
  /// <param name="token">A data token</param>
  /// <example>
  /// *Example:* Terminating all connections with a custom error message.
  /// 
  /// ```csharp
  /// void DisconnectAll(int errorCode, string errorMessage) {
  ///   ServerMessage msg = new ServerMessage(errorCode, errorMessage);
  ///   
  ///   foreach(var connection in BoltNetwork.connections) {
  ///     connection.Disconnect(errorMessage);
  ///   }
  /// }
  /// ```
  /// </example>
  public void Disconnect(IProtocolToken token) {
    _udp.Disconnect(token.ToByteArray());
  }

  public int GetSkippedUpdates(BoltEntity en) {
    return _entityChannel.GetSkippedUpdates(en.Entity);
  }

  /// <summary>
  /// Reference comparison between two connections
  /// </summary>
  /// <param name="obj">The object to compare</param>
  /// <example>
  /// bool Compare(BoltConnection A, BoltConnection B) {
  ///   return A.Equals(B);
  /// }
  /// </example>
  public override bool Equals(object obj) {
    return ReferenceEquals(this, obj);
  }

  public bool IsScoped(BoltEntity entity) {
    return _entityChannel.MightExistOnRemote(entity.Entity);
  }

  /// <summary>
  /// A hash code for this connection
  /// </summary>
  public override int GetHashCode() {
    return _udp.GetHashCode();
  }
  
  /// <summary>
  /// The string representation of this connection
  /// </summary>
  /// <example>
  /// *Example:* Logging the address of new connections using the string representation.
  /// 
  /// ```csharp
  /// public override void Connected(BoltConnection connection) {
  ///   ServerLog.instance.Write("New Connection:" + connection.ToString());
  /// }
  /// ```
  /// </example>
  public override string ToString() {
    return string.Format("[Connection {0}]", _udp.RemoteEndPoint);
  }

  internal void DisconnectedInternal() {
    for (int i = 0; i < _channels.Length; ++i) {
      _channels[i].Disconnected();
    }

    if (UserData != null) {
      if (UserData is IDisposable) {
        (UserData as IDisposable).Dispose();
      }

      UserData = null;
    }
  }

  internal bool StepRemoteFrame() {
    if (_framesToStep > 0) {
      _framesToStep -= 1;
      _remoteFrameEstimated += 1;

      for (int i = 0; i < _channels.Length; ++i) {
        _channels[i].StepRemoteFrame();
      }
    }

    return _framesToStep > 0;
  }

  internal void AdjustRemoteFrame() {
    if (_packetsReceived == 0) {
      return;
    }

    if (BoltCore._config.disableDejitterBuffer) {
      if (_remoteFrameAdjust) {
        _framesToStep = Mathf.Max(0, _remoteFrameActual - _remoteFrameEstimated);
        _remoteFrameEstimated = _remoteFrameActual;
        _remoteFrameAdjust = false;
      }
      else {
        _framesToStep = 1;
      }

      return;
    }

    int rate = BoltCore.remoteSendRate;
    int delay = BoltCore.localInterpolationDelay;
    int delayMin = BoltCore.localInterpolationDelayMin;
    int delayMax = BoltCore.localInterpolationDelayMax;

    bool useDelay = delay >= 0;

    if (_remoteFrameAdjust) {
      _remoteFrameAdjust = false;

      // if we apply delay
      if (useDelay) {
        // first packet is special!
        if (_packetsReceived == 1) {
          _remoteFrameEstimated = _remoteFrameActual - delay;
        }

        // calculate frame diff (actual vs. estimated)
        _remoteFrameDiff = _remoteFrameActual - _remoteFrameEstimated;

        // if we are *way* off
        if ((_remoteFrameDiff < (delayMin - rate)) || (_remoteFrameDiff > (delayMax + rate))) {
          int oldFrame = _remoteFrameEstimated;
          int newFrame = _remoteFrameActual - delay;

          BoltLog.Debug("FRAME RESET: {0}", _remoteFrameDiff);

          _remoteFrameEstimated = newFrame;
          _remoteFrameDiff = _remoteFrameActual - _remoteFrameEstimated;

          // call into channels to notify that the frame reset
          for (int i = 0; i < _channels.Length; ++i) {
            _channels[i].RemoteFrameReset(oldFrame, newFrame);
          }
        }
      }
    }

    if (useDelay) {
      // drifted to far behind, step two frames
      if (_remoteFrameDiff > delayMax) {
        BoltLog.Debug("FRAME FORWARD: {0}", _remoteFrameDiff);
        _framesToStep = 2;
        _remoteFrameDiff -= _framesToStep;
      }

      // drifting to close to 0 delay, stall one frame
      else if (_remoteFrameDiff < delayMin) {
        BoltLog.Debug("FRAME STALL: {0}", _remoteFrameDiff);
        _framesToStep = 0;
        _remoteFrameDiff += 1;
      }

      // we have not drifted, just step one frame
      else {
        _framesToStep = 1;
      }
    }
    else {
      _remoteFrameEstimated = _remoteFrameActual - (rate - 1);
    }
  }

  internal void SwitchPerfCounters() {
    _bitsSecondOut = _bitsSecondOutAcc;
    _bitsSecondOutAcc = 0;

    _bitsSecondIn = _bitsSecondInAcc;
    _bitsSecondInAcc = 0;
  }

  internal void Send() {
    try {

      Packet packet = PacketPool.Acquire();
      packet.Frame = BoltCore.frame;
      packet.Number = ++_packetCounter;

      packet.UdpPacket = BoltCore.AllocateUdpPacket();
      packet.UdpPacket.UserToken = packet;
      packet.UdpPacket.WriteIntVB(packet.Frame);

      for (int i = 0; i < _channels.Length; ++i) {
        _channels[i].Pack(packet);
      }

      Assert.False(packet.UdpPacket.Overflowing);

      _udp.Send(packet.UdpPacket);

      _bitsSecondOutAcc += packet.UdpPacket.Position;
      _packetStatsOut.Enqueue(packet.Stats);
    }
    catch (Exception exn) {
      BoltLog.Exception(exn);
      throw;
    }
  }

  internal void PacketReceived(UdpPacket udpPacket) {
    try {
      using (Packet packet = PacketPool.Acquire()) {
        packet.UdpPacket = udpPacket;
        packet.Frame = packet.UdpPacket.ReadIntVB();

        if (packet.Frame > _remoteFrameActual) {
          _remoteFrameAdjust = true;
          _remoteFrameActual = packet.Frame;
        }

        _bitsSecondInAcc += packet.UdpPacket.Size;
        _packetsReceived += 1;

        for (int i = 0; i < _channels.Length; ++i) {
          _channels[i].Read(packet);
        }

        for (int i = 0; i < _channels.Length; ++i) {
          _channels[i].ReadDone();
        }

        _packetStatsIn.Enqueue(packet.Stats);

        Assert.False(udpPacket.Overflowing);
      }
    }
    catch (Exception exn) {
      BoltLog.Exception(exn);
      BoltLog.Error("exception thrown while unpacking data from {0}, disconnecting", udpConnection.RemoteEndPoint);
      Disconnect();
    }
  }

  internal void PacketDelivered(Packet packet) {
    try {
      for (int i = 0; i < _channels.Length; ++i) {
        _channels[i].Delivered(packet);
      }
    }
    catch (Exception exn) {
      BoltLog.Exception(exn);
      BoltLog.Error("exception thrown while handling delivered packet to {0}", udpConnection.RemoteEndPoint);
    }
  }

  internal void PacketLost(Packet packet) {
    try {
      for (int i = 0; i < _channels.Length; ++i) {
        _channels[i].Lost(packet);
      }
    }
    catch (Exception exn) {
      BoltLog.Exception(exn);
      BoltLog.Error("exception thrown while handling lost packet to {0}", udpConnection.RemoteEndPoint);
    }
  }

  public static implicit operator bool(BoltConnection cn) {
    return cn != null;
  }
}
