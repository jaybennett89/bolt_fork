using Bolt;
using System;
using System.Collections.Generic;
using UdpKit;
using UnityEngine;

/// <summary>
/// Represents a connection from a remote host
/// </summary>
public class BoltConnection : BoltObject {
  static int _idCounter;

  UdpConnection _udp;
  BoltChannel[] _channels;

  int _id;
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

  internal SceneLoadState _remoteMapLoadState;
  internal BoltEventChannel _eventChannel;
  internal BoltEntityChannel _entityChannel;
  internal BoltEntityChannel.CommandChannel _commandChannel;

  public bool isLoadingMap {
    get { return _remoteMapLoadState.stage != SceneLoadStage.CallbackDone; }
  }

  /// <summary>
  /// Unique id for this connection
  /// </summary>
  public int id {
    get { return _id; }
  }

  /// <summary>
  /// The underlying UdpKit connection for this connection
  /// </summary>
  public UdpConnection udpConnection {
    get { return _udp; }
  }

  /// <summary>
  /// This is the estimated remote frame of the other end of this connection
  /// </summary>
  public int remoteFrame {
    get { return _remoteFrameEstimated; }
  }

  /// <summary>
  /// Ping (in seconds) of this connection
  /// </summary>
  public float ping {
    get { return _udp.NetworkPing; }
  }

  /// <summary>
  /// Aliased ping (in seconds) of this connection
  /// </summary>
  public float pingAliased {
    get { return _udp.AliasedPing; }
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
  /// User setable token
  /// </summary>
  public object userToken {
    get;
    set;
  }

  internal BoltConnection (UdpConnection udp) {
    userToken = udp.UserToken;

    _udp = udp;
    _udp.UserToken = this;

    _id = ++_idCounter;
    _remoteFrameAdjust = false;

    _channels = new BoltChannel[] {
      _eventChannel = new BoltEventChannel(),
      _commandChannel = new BoltEntityChannel.CommandChannel(),
      _entityChannel = new BoltEntityChannel(),
    };

    // set channels connection
    for (int i = 0; i < _channels.Length; ++i) {
      _channels[i].connection = this;
    }
  }

  /// <summary>
  /// Disconnect this connection
  /// </summary>
  public void Disconnect () {
    _udp.Disconnect();
  }

  /// <summary>
  /// How many updates have been skipped for the entity to this connection
  /// </summary>
  public int GetSkippedUpdates (BoltEntity en) {
    return _entityChannel.GetSkippedUpdates(en.Entity);
  }

  internal NetId GetNetworkId (BoltEntity en) {
    return _entityChannel.GetNetworkId(en.Entity);
  }

  /// <summary>
  /// Raise an event on the remote end of this connection
  /// </summary>
  /// <param name="event">The event to raise</param>
  public void Raise (IBoltEvent @event) {
    _eventChannel.Queue((BoltEventBase) @event);
  }

  ///// <summary>
  ///// Raise an event of type T on the remote end of this connection
  ///// </summary>
  ///// <typeparam name="T">Type of the event</typeparam>
  ///// <param name="init">Function called to setup the event before its sent of</param>
  internal void Raise<T> (Action<T> init) where T : IBoltEvent {
    T evnt = BoltFactory.NewEvent<T>();
    init(evnt);
    Raise(evnt);
  }

  public override bool Equals (object obj) {
    return ReferenceEquals(this, obj);
  }

  public override int GetHashCode () {
    return _udp.GetHashCode();
  }

  public override string ToString () {
    return string.Format("[Connection addr={0} port={1}]", _udp.RemoteEndPoint.Address, _udp.RemoteEndPoint.Port);
  }

  internal BoltEntity GetIncommingEntity (NetId networkId) {
    return _entityChannel.GetIncommingEntity(networkId).UnityObject as BoltEntity;
  }

  internal BoltEntity GetOutgoingEntity(NetId networkId) {
    return _entityChannel.GetOutgoingEntity(networkId).UnityObject as BoltEntity;
  }

  internal void DisconnectedInternal () {
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

  internal void LoadMapOnClient (Scene map) {
    Assert.True(udpConnection.IsServer);
    _remoteMapLoadState = _remoteMapLoadState.BeginLoad(map);

    if (_remoteMapLoadState.stage == SceneLoadStage.Load) {
      SendMapLoadToRemote();
    }
  }

  internal void SendMapLoadDoneToRemote () {
    Assert.True(BoltCore._mapLoadState.stage >= SceneLoadStage.Callback);
    Assert.True(BoltCore._mapLoadState.scene == _remoteMapLoadState.scene);

    Raise<ILoadMapDone>(evt => evt.map = BoltCore._mapLoadState.scene);
  }

  internal void SendMapLoadToRemote () {
    Assert.True(udpConnection.IsServer);
    Raise<ILoadMap>(evt => evt.map = _remoteMapLoadState.scene);

    if (BoltCore._mapLoadState.stage == SceneLoadStage.CallbackDone) {
      SendMapLoadDoneToRemote();
    }
  }

  internal void TriggerRemoteMapDoneCallbacks () {
    _remoteMapLoadState = _remoteMapLoadState.BeginCallback(BoltCore._mapLoadState);

    if (_remoteMapLoadState.stage == SceneLoadStage.Callback) {
      // invoke
      BoltCallbacksBase.SceneLoadRemoteDoneInvoke(this, _remoteMapLoadState.scene.name);

      // done!
      _remoteMapLoadState = _remoteMapLoadState.FinishCallback(BoltCore._mapLoadState.scene);

      // this gotta be true now
      Assert.True(_remoteMapLoadState.stage == SceneLoadStage.CallbackDone);
    }
  }

  internal bool StepRemoteFrame () {
    if (_framesToStep > 0) {
      _framesToStep -= 1;
      _remoteFrameEstimated += 1;

      for (int i = 0; i < _channels.Length; ++i) {
        _channels[i].StepRemoteFrame();
      }
    }

    return _framesToStep > 0;
  }

  internal void AdjustRemoteFrame () {
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
    } else {
      _remoteFrameEstimated = _remoteFrameActual - (rate - 1);
    }
  }

  internal void SwitchPerfCounters () {
    _bitsSecondOut = _bitsSecondOutAcc;
    _bitsSecondOutAcc = 0;

    _bitsSecondIn = _bitsSecondInAcc;
    _bitsSecondInAcc = 0;
  }

  internal void Send () {
    try {
      BoltPacket packet = BoltPacketPool.Acquire();
      packet.info = new BoltPacketInfo();
      packet.number = ++_packetCounter;
      packet.frame = BoltCore.frame;
      packet.stream.WriteInt(packet.frame);

      for (int i = 0; i < _channels.Length; ++i) {
        _channels[i].Pack(packet);
      }

      Assert.False(packet.stream.Overflowing);

      _bitsSecondOutAcc += packet.stream.Position;
      _udp.Send(packet);

      //BoltLog.Info(
      //    "Sent packet of {0} bits, cmd: {1}, entity: {2}, events: {3}",
      //    packet.stream.Position,
      //    packet.info.commandBits,
      //    packet.info.entityBits,
      //    packet.info.eventBits
      //);

    } catch (Exception exn) {
      BoltLog.Exception(exn);
      throw;
    }
  }

  internal void PacketSent (BoltPacket packet) {
    for (int i = 0; i < _channels.Length; ++i) {
      _channels[i].Sent(packet);
    }
  }

  internal void PacketReceived (BoltPacket packet) {
    try {
      packet.frame = packet.stream.ReadInt();
      //BoltLog.Info("PACKET-FRAME: {0}", packet.frame);

      if (packet.frame > _remoteFrameActual) {
        _remoteFrameAdjust = true;
        _remoteFrameActual = packet.frame;
      }

      _packetsReceived += 1;
      _bitsSecondInAcc += packet.stream.Size;

      for (int i = 0; i < _channels.Length; ++i) {
        _channels[i].Read(packet);
      }

      for (int i = 0; i < _channels.Length; ++i) {
        _channels[i].ReadDone();
      }

      Assert.False(packet.stream.Overflowing);
    } catch (Exception exn) {
      BoltLog.Exception(exn);
      BoltLog.Error("exception thrown while unpacking data from {0}, disconnecting", udpConnection.RemoteEndPoint);

      Disconnect();
    }
  }

  internal void PacketDelivered (BoltPacket packet) {
    for (int i = 0; i < _channels.Length; ++i) {
      _channels[i].Delivered(packet);
    }
  }

  internal void PacketLost (BoltPacket packet) {
    for (int i = 0; i < _channels.Length; ++i) {
      _channels[i].Lost(packet);
    }
  }

  public static implicit operator bool (BoltConnection cn) {
    return cn != null;
  }

}
