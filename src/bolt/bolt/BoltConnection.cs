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

  internal bool _canReceiveEntities = true;
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

  public IProtocolToken ConnectToken {
    get;
    internal set;
  }

  public IProtocolToken AcceptToken {
    get;
    internal set;
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

  public uint ConnectionId {
    get { return udpConnection.ConnectionId; }
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

  public void StreamBytes(UdpChannelName channel, byte[] data) {
    _udp.StreamBytes(channel, data);
  }

  public void SetCanReceiveEntities(bool canReceive) {
    _canReceiveEntities = canReceive;
  }

  public void Disconnect() {
    Disconnect(null);
  }

  /// <summary>
  /// Disconnect this connection
  /// </summary>
  public void Disconnect(IProtocolToken token) {
    _udp.Disconnect(token.ToByteArray());
  }

  public int GetSkippedUpdates(BoltEntity en) {
    return _entityChannel.GetSkippedUpdates(en.Entity);
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

  int notifyPacketNumber = 0;

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
      packet.UdpPacket.WriteInt(packet.Frame);

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

  //internal void PacketReceived(BoltPacket packet) {
  internal void PacketReceived(UdpPacket udpPacket) {
    try {
      using (Packet packet = PacketPool.Acquire()) {
        packet.UdpPacket = udpPacket;
        packet.Frame = packet.UdpPacket.ReadInt();

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
      Assert.True((notifyPacketNumber + 1) == packet.Number, "notify packet number did not match");
      notifyPacketNumber = packet.Number;
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
      Assert.True((notifyPacketNumber + 1) == packet.Number, "notify packet number did not match");
      notifyPacketNumber = packet.Number;
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
