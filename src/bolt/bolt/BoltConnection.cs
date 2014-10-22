using Bolt;
using System;
using System.Collections.Generic;
using UdpKit;
using UnityEngine;

public struct PacketStats {
  public int StateBits;
  public int EventBits;
  public int CommandBits;
}

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

  internal bool _canReceiveEntities = false;
  internal SceneLoadState _remoteSceneLoading;

  /// <summary>
  /// Returns true if the remote computer on the other end of this connection is loading a map currently
  /// </summary>
  public bool isLoadingMap {
    get {
      return
        (_remoteSceneLoading.Scene != BoltCore._localSceneLoading.Scene) ||
        (_remoteSceneLoading.State != SceneLoadState.STATE_CALLBACK_INVOKED);
    }
  }

  public int remoteFrame {
    get { return _remoteFrameEstimated; }
  }

  [Obsolete("Use BoltConnection.pingNetwork instead")]
  public float ping {
    get { return _udp.NetworkPing; }
  }

  public float pingNetwork {
    get { return _udp.NetworkPing; }
  }

  public int dejitterFrames {
    get { return _remoteFrameActual - _remoteFrameEstimated; }
  }

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
  public int bitsPerSecondIn {
    get { return _bitsSecondIn; }
  }

  /// <summary>
  /// How many bits per second we are sending out
  /// </summary>
  public int bitsPerSecondOut {
    get { return _bitsSecondOut; }
  }

  /// <summary>
  /// Remote end point of this connection
  /// </summary>
  public UdpEndPoint remoteEndPoint {
    get { return udpConnection.RemoteEndPoint; }
  }

  /// <summary>
  /// User assignable token which lets you pair arbitrary data with the connection
  /// </summary>
  public object userToken {
    get;
    set;
  }

  internal BoltConnection(UdpConnection udp) {
    userToken = udp.UserToken;

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
  /// Disconnect this connection
  /// </summary>
  public void Disconnect() {
    _udp.Disconnect();
  }

  /// <summary>
  /// How many updates have been skipped for the entity to this connection
  /// </summary>
  public int GetSkippedUpdates(BoltEntity en) {
    return _entityChannel.GetSkippedUpdates(en.Entity);
  }

  internal NetId GetNetworkId(Entity en) {
    return _entityChannel.GetNetworkId(en);
  }

  public override bool Equals(object obj) {
    return ReferenceEquals(this, obj);
  }

  public bool IsScoped(BoltEntity entity) {
    return _entityChannel.MightExistOnRemote(entity.Entity);
  }

  public override int GetHashCode() {
    return _udp.GetHashCode();
  }

  public override string ToString() {
    return string.Format("[Connection {0}]", _udp.RemoteEndPoint);
  }

  internal Entity GetIncommingEntity(NetId networkId) {
    return _entityChannel.GetIncommingEntity(networkId);
  }

  internal Entity GetOutgoingEntity(NetId networkId) {
    return _entityChannel.GetOutgoingEntity(networkId);
  }

  internal void DisconnectedInternal() {
    for (int i = 0; i < _channels.Length; ++i) {
      _channels[i].Disconnected();
    }

    if (userToken != null) {
      if (userToken is IDisposable) {
        (userToken as IDisposable).Dispose();
      }

      userToken = null;
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
    if (_packetsReceived == 0)
      return;

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
      BoltPacket packet = BoltPacketPool.Acquire();
      packet.stats = new PacketStats();
      packet.number = ++_packetCounter;
      packet.frame = BoltCore.frame;
      packet.stream.WriteInt(packet.frame);

      for (int i = 0; i < _channels.Length; ++i) {
        _channels[i].Pack(packet);
      }

      Assert.False(packet.stream.Overflowing);

      _udp.Send(packet);

      _bitsSecondOutAcc += packet.stream.Position;
      _packetStatsOut.Enqueue(packet.stats);
    }
    catch (Exception exn) {
      BoltLog.Exception(exn);
      throw;
    }
  }

  internal void PacketSent(BoltPacket packet) {
    for (int i = 0; i < _channels.Length; ++i) {
      _channels[i].Sent(packet);
    }
  }

  internal void PacketReceived(BoltPacket packet) {
    try {
      packet.frame = packet.stream.ReadInt();
      packet.stats = new PacketStats();

      if (packet.frame > _remoteFrameActual) {
        _remoteFrameAdjust = true;
        _remoteFrameActual = packet.frame;
      }

      _bitsSecondInAcc += packet.stream.Size;
      _packetsReceived += 1;

      for (int i = 0; i < _channels.Length; ++i) {
        _channels[i].Read(packet);
      }

      for (int i = 0; i < _channels.Length; ++i) {
        _channels[i].ReadDone();
      }

      _packetStatsIn.Enqueue(packet.stats);
      Assert.False(packet.stream.Overflowing);
    }
    catch (Exception exn) {
      BoltLog.Exception(exn);
      BoltLog.Error("exception thrown while unpacking data from {0}, disconnecting", udpConnection.RemoteEndPoint);

      Disconnect();
    }
  }

  internal void PacketDelivered(BoltPacket packet) {
    for (int i = 0; i < _channels.Length; ++i) {
      _channels[i].Delivered(packet);
    }
  }

  internal void PacketLost(BoltPacket packet) {
    for (int i = 0; i < _channels.Length; ++i) {
      _channels[i].Lost(packet);
    }
  }

  public static implicit operator bool(BoltConnection cn) {
    return cn != null;
  }

}
