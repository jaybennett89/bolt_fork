using Bolt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UdpKit;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;
using UE = UnityEngine;

public enum BoltNetworkModes {
  None = 0,
  Host = 1,
  [Obsolete("Use BoltNetworkModes.Host instead")]
  Server = 1,
  Client = 2,
  Shutdown = 3,
}

internal static class BoltCore {
  internal static UdpSocket _udpSocket;
  internal static UdpPlatform _udpPlatform;

  static internal string _autoloadScene = "";
  static internal Stopwatch _timer = new Stopwatch();
  static internal SceneLoadState _localSceneLoading;

  static internal bool _canReceiveEntities = true;
  static internal IPrefabPool PrefabPool = new DefaultPrefabPool();
  static internal IEventFilter EventFilter = new DefaultEventFilter();

  static internal int _frame = 0;
  static internal BoltNetworkModes _mode = BoltNetworkModes.None;

  static internal BoltConfig _config = null;
  static internal UdpConfig _udpConfig = null;

  static internal BoltDoubleList<Entity> _entitiesOK = new BoltDoubleList<Entity>();
  static internal BoltDoubleList<Entity> _entitiesFZ = new BoltDoubleList<Entity>();

  static internal IEnumerable<Entity> _entities {
    get {
      var it = _entitiesOK.GetIterator();

      while (it.Next()) {
        yield return it.val;
      }

      it = _entitiesFZ.GetIterator();

      while (it.Next()) {
        yield return it.val;
      }
    }
  }

  static internal BoltDoubleList<BoltConnection> _connections = new BoltDoubleList<BoltConnection>();
  static internal Bolt.EventDispatcher _globalEventDispatcher = new Bolt.EventDispatcher();
  static internal Dictionary<UniqueId, BoltEntity> _sceneObjects = new Dictionary<UniqueId, BoltEntity>(UniqueId.EqualityComparer.Instance);

  static internal GameObject _globalControlObject = null;
  static internal GameObject _globalBehaviourObject = null;
  static internal List<STuple<BoltGlobalBehaviourAttribute, Type>> _globalBehaviours = new List<STuple<BoltGlobalBehaviourAttribute, Type>>();

  public static bool isDebugMode {
#if DEBUG
    get { return true; }
#else
    get { return false; }
#endif
  }

  internal static Func<GameObject, Vector3, Quaternion, GameObject> _instantiate =
    (go, p, r) => (GameObject)UnityEngine.GameObject.Instantiate(go, p, r);

  internal static Action<GameObject> _destroy =
    (go) => GameObject.Destroy(go);

  public static int loadedScene {
    get { return _localSceneLoading.Scene.Index; }
  }

  public static string loadedSceneName {
    get { return BoltNetworkInternal.GetSceneName(_localSceneLoading.Scene.Index); }
  }

  public static GameObject globalObject {
    get { return _globalBehaviourObject; }
  }

  public static IEnumerable<BoltEntity> entities {
    get { return _entities.Select(x => x.UnityObject); }
  }

  public static IEnumerable<BoltConnection> connections {
    get { return _connections; }
  }

  public static IEnumerable<BoltConnection> clients {
    get { return connections.Where(c => c.udpConnection.IsServer); }
  }

  public static BoltConnection server {
    get { return connections.FirstOrDefault(c => c.udpConnection.IsClient); }
  }

  public static int frame {
    get { return _frame; }
  }

  public static int framesPerSecond {
    get { return _config.framesPerSecond; }
  }

  public static int serverFrame {
    get { return _mode == BoltNetworkModes.None ? 0 : (isServer ? _frame : server.RemoteFrame); }
  }

  public static float serverTime {
    get { return ((float)serverFrame) / ((float)framesPerSecond); }
  }

  public static float time {
    get { return Time.time; }
  }

  public static float frameBeginTime {
    get { return Time.fixedTime; }
  }

  public static float frameDeltaTime {
    get { return Time.fixedDeltaTime; }
  }

  public static float frameAlpha {
    get { return Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime); }
  }

  public static bool isClient {
    get { return hasSocket && _mode == BoltNetworkModes.Client; }
  }

  public static bool isServer {
    get { return hasSocket && _mode == BoltNetworkModes.Host; }
  }

  internal static int localSendRate {
    get {
      switch (_mode) {
        case BoltNetworkModes.Host: return _config.serverSendRate;
        case BoltNetworkModes.Client: return _config.clientSendRate;
        default: return -1;
      }
    }
  }

  internal static int remoteSendRate {
    get {
      switch (_mode) {
        case BoltNetworkModes.Host: return _config.clientSendRate;
        case BoltNetworkModes.Client: return _config.serverSendRate;
        default: return -1;
      }
    }
  }

  internal static int localInterpolationDelay {
    get {
      switch (_mode) {
        case BoltNetworkModes.Host: return _config.serverDejitterDelay;
        case BoltNetworkModes.Client: return _config.clientDejitterDelay;
        default: return -1;
      }
    }
  }

  internal static int localInterpolationDelayMin {
    get {
      switch (_mode) {
        case BoltNetworkModes.Host: return _config.serverDejitterDelayMin;
        case BoltNetworkModes.Client: return _config.clientDejitterDelayMin;
        default: return -1;
      }
    }
  }

  internal static int localInterpolationDelayMax {
    get {
      switch (_mode) {
        case BoltNetworkModes.Host: return _config.serverDejitterDelayMax;
        case BoltNetworkModes.Client: return _config.clientDejitterDelayMax;
        default: return -1;
      }
    }
  }

  internal static bool hasSocket {
    get { return _udpSocket != null; }
  }

  public static void Destroy(BoltEntity entity, IProtocolToken detachToken) {
    if (!entity.isOwner) {
      BoltLog.Warn("Only the owner can destroy an entity, ignoring call to Destroy().");
      return;
    }

    if (!entity.isAttached) {
      BoltLog.Warn("Entity is not attached, ignoring call to Destroy().");
      return;
    }

    entity.Entity.DetachToken = detachToken;
    DestroyForce(entity.Entity);
  }

  internal static void DestroyForce(Bolt.Entity entity) {
    // detach
    entity.Detach();

    // destroy
    PrefabPool.Destroy(entity.UnityObject.gameObject);
  }

  internal static BoltEntity Instantiate(PrefabId prefabId, TypeId serializerId, UE.Vector3 position, UE.Quaternion rotation, InstantiateFlags instanceFlags, BoltConnection controller, IProtocolToken attachToken) {
    // prefab checks
    GameObject prefab = PrefabPool.LoadPrefab(prefabId);
    BoltEntity entity = prefab.GetComponent<BoltEntity>();

    if (isClient && (entity._allowInstantiateOnClient == false)) {
      throw new BoltException("This prefab is not allowed to be instantiated on clients");
    }

    if (entity._prefabId != prefabId.Value) {
      throw new BoltException("PrefabId for BoltEntity component did not return the same value as prefabId passed in as argument to Instantiate");
    }

    Entity eo;
    eo = Entity.CreateFor(prefabId, serializerId, position, rotation);
    eo.Initialize();
    eo.AttachToken = attachToken;
    eo.Attach();

    return eo.UnityObject;
  }

  internal static GameObject Attach(GameObject gameObject, EntityFlags flags) {
    return Attach(gameObject, flags, null);
  }

  internal static GameObject Attach(GameObject gameObject, EntityFlags flags, IProtocolToken attachToken) {
    BoltEntity be = gameObject.GetComponent<BoltEntity>();
    return Attach(gameObject, Factory.GetFactory(be.serializerGuid).TypeId, flags, attachToken);
  }

  internal static GameObject Attach(GameObject gameObject, TypeId serializerId, EntityFlags flags, IProtocolToken attachToken) {
    BoltEntity be = gameObject.GetComponent<BoltEntity>();

    Entity en;
    en = Entity.CreateFor(gameObject, new PrefabId(be._prefabId), serializerId, flags);
    en.Initialize();
    en.AttachToken = attachToken;
    en.Attach();

    return en.UnityObject.gameObject;
  }

  public static void Detach(BoltEntity entity, IProtocolToken detachToken) {
    Assert.NotNull(entity.Entity);
    entity.Entity.DetachToken = detachToken;
    entity.Entity.Detach();
  }

  public static Bolt.Entity FindEntity(NetworkId id) {
    // remap network id to local id
    if ((id.Connection == uint.MaxValue) && (NetworkIdAllocator.LocalConnectionId != uint.MaxValue)) {
      id = new NetworkId(NetworkIdAllocator.LocalConnectionId, id.Entity);
    }

    foreach(var itval in _entities) {
      if (itval.NetworkId == id) {
        return itval;
      }
    }

    return null;
  }

  public static void LoadScene(int index, IProtocolToken token) {
    if (isServer == false) {
      BoltLog.Error("You are not the server, only the server can initiate a scene load");
      return;
    }

    // pass to internal call
    LoadSceneInternal(_localSceneLoading.BeginLoad(index, token));
  }


  internal static void LoadSceneInternal(SceneLoadState loading) {
    // update
    _localSceneLoading = loading;

    // begin loading
    BoltSceneLoader.Enqueue(_localSceneLoading);
  }

  public static void Shutdown() {
    if (_globalControlObject) {
      _globalControlObject.SendMessage("QueueShutdown", new ControlCommandShutdown());
    }
    else {
      throw new BoltException("Could not find BoltControl object");
    }
  }

  public static UdpSession[] GetSessions() {
    return _udpSocket.GetSessions();
  }

  public static void Connect(UdpEndPoint endpoint, IProtocolToken token) {
    if (server != null) {
      BoltLog.Error("You must disconnect from the current server first");
      return;
    }

    // stop broadcasting
    DisableLanBroadcast();

    // connect
    _udpSocket.Connect(endpoint, token.ToByteArray());
  }

  public static void Connect(UdpSession session, IProtocolToken token) {
    if (server != null) {
      BoltLog.Error("You must disconnect from the current server first");
      return;
    }

    // stop broadcasting
    DisableLanBroadcast();

    // connect
    _udpSocket.Connect(session, token.ToByteArray());
  }

  public static void SetHostInfo(string serverName, bool dedicated, IProtocolToken token) {
    if (BoltCore.isServer == false) {
      BoltLog.Error("Only the server can call SetSessionData");
      return;
    }

    _udpSocket.SetHostInfo(serverName, dedicated, token.ToByteArray());
  }

  public static void EnableLanBroadcast(UdpIPv4Address local, UdpIPv4Address broadcast, ushort port) {
    _udpSocket.LanBroadcastEnable(local, broadcast, port);
  }

  public static void DisableLanBroadcast() {
    _udpSocket.LanBroadcastDisable();
  }

  static void AdjustEstimatedRemoteFrames() {
    if (hasSocket) {
      BoltIterator<BoltConnection> it = _connections.GetIterator();

      while (it.Next()) {
        it.val.AdjustRemoteFrame();
      }
    }
  }

  static void StepNonControlledRemoteEntities() {
    if (hasSocket) {
      bool retry;

      do {
        retry = false;
        BoltIterator<BoltConnection> it = _connections.GetIterator();

        while (it.Next()) {
          if (it.val.StepRemoteEntities()) {
            retry = true;
          }
        }
      } while (retry);
    }
  }

  static void PollNetwork() {
    UdpEvent ev;

    while (hasSocket && _udpSocket.Poll(out ev)) {
      switch (ev.EventType) {
        case UdpEventType.SocketStartupDone:
          Udp_SocketStartupDone(ev.As<UdpEventStartDone>());
          break;

        case UdpEventType.SocketStartupFailed:
          Udp_SocketStartupFailed(ev.As<UdpEventStartFailed>());
          break;

        case UdpEventType.Connected:
          Udp_Connected((UdpConnection)ev.Object0);
          break;

        case UdpEventType.Disconnected:
          Udp_Disconnect((UdpConnection)ev.Object0);
          break;

        case UdpEventType.ConnectRequest:
          Udp_ConnectRequest(ev.As<UdpEventConnectRequest>());
          break;

        case UdpEventType.ConnectFailed:
          Udp_ConnectFailed(ev.As<UdpEventConnectFailed>());
          break;

        case UdpEventType.ConnectRefused:
          Udp_ConnectRefused(ev.As<UdpEventConnectRefused>());
          break;

        case UdpEventType.ConnectAttempt:
          Udp_ConnectAttempt(ev.As<UdpEventConnectAttempt>());
          break;

        case UdpEventType.PacketLost:
          Udp_PacketLost(ev);
          break;

        case UdpEventType.PacketDelivered:
          Udp_PacketDelivered(ev);
          break;

        case UdpEventType.PacketReceived:
          Udp_PacketReceived(ev);
          break;

        case UdpEventType.SessionListUpdated:
          Udp_SessionListUpdated(ev);
          break;

        case UdpEventType.SessionConnectFailed:
          Udp_SessionConnectFailed(ev.As<UdpEventSessionConnectFailed>());
          break;

        case UdpEventType.MasterServerConnected:
          Udp_MasterServerConnected(ev.As<UdpEventMasterServerConnected>());
          break;

        case UdpEventType.MasterServerDisconnected:
          Udp_MasterServerDisconnected(ev.As<UdpEventMasterServerDisconnected>());
          break;

        case UdpEventType.MasterServerConnectFailed:
          Udp_MasterServerConnectFailed(ev.As<UdpEventMasterServerConnectFailed>());
          break;

        case UdpEventType.MasterServerNatProbeResult:
          Udp_MasterServerNatProbeResult(ev.As<UdpEventMasterServerNatFeatures>());
          break;

        case UdpEventType.StreamDataReceived:
          Udp_StreamDataReceived(ev);
          break;
      }
    }
  }

  static void Udp_MasterServerNatProbeResult(UdpEventMasterServerNatFeatures ev) {
    BoltInternal.GlobalEventListenerBase.ZeusNatProbeResultInvoke(ev.Features);
  }

  static void Udp_MasterServerConnectFailed(UdpEventMasterServerConnectFailed ev) {
    BoltInternal.GlobalEventListenerBase.ZeusConnectFailedInvoke(ev.EndPoint);
  }

  static void Udp_MasterServerDisconnected(UdpEventMasterServerDisconnected ev) {
    BoltInternal.GlobalEventListenerBase.ZeusDisconnectedInvoke(ev.EndPoint);
  }

  static void Udp_MasterServerConnected(UdpEventMasterServerConnected ev) {
    BoltInternal.GlobalEventListenerBase.ZeusConnectedInvoke(ev.EndPoint);
  }

  static void Udp_SessionConnectFailed(UdpEventSessionConnectFailed ev) {
    BoltInternal.GlobalEventListenerBase.SessionConnectFailedInvoke(ev.Session, ev.Token.ToToken());
  }

  static void Udp_SessionListUpdated(UdpEvent ev) {
    // store session list
    BoltNetwork._sessionList = (Map<Guid, UdpSession>)ev.Object0;

    // notify user
    BoltInternal.GlobalEventListenerBase.SessionListUpdatedInvoke(BoltNetwork._sessionList);
  }

  static void Udp_StreamDataReceived(UdpEvent ev) {
    var c = ev.Object0 as UdpConnection;
    var s = ev.Object1 as UdpStreamData;
    BoltInternal.GlobalEventListenerBase.StreamDataReceivedInvoke(c.GetBoltConnection(), s);
  }

  static void Udp_PacketReceived(UdpEvent ev) {
    var p = (UdpPacket)ev.Object1;
    var c = (UdpConnection)ev.Object0;

    c.GetBoltConnection().PacketReceived(p);
  }

  static void Udp_PacketDelivered(UdpEvent ev) {
    var p = (UdpPacket)ev.Object1;
    var c = (UdpConnection)ev.Object0;

    using (var packet = (Packet)p.UserToken) {
      c.GetBoltConnection().PacketDelivered(packet);
    }
  }

  static void Udp_PacketLost(UdpEvent ev) {
    var p = (UdpPacket)ev.Object1;
    var c = (UdpConnection)ev.Object0;

    using (var packet = (Packet)p.UserToken) {
      c.GetBoltConnection().PacketLost(packet);
    }
  }

  static void Udp_ConnectAttempt(UdpEventConnectAttempt ev) {
    BoltInternal.GlobalEventListenerBase.ConnectAttemptInvoke(ev.EndPoint, ev.Token.ToToken());
  }

  static void Udp_SocketStartupFailed(UdpEventStartFailed ev) {
    // global callback
    BoltInternal.GlobalEventListenerBase.BoltStartFailedInvoke();

    // reset event
    ev.ResetEvent.Set();
  }

  static void Udp_SocketStartupDone(UdpEventStartDone ev) {
    // global callback
    BoltInternal.GlobalEventListenerBase.BoltStartDoneInvoke();

    // flag reset event
    ev.ResetEvent.Set();

    // auto load scene
    if (_autoloadScene != null) {
      BoltNetwork.LoadScene(_autoloadScene);
    }
  }

  static void Udp_ConnectFailed(UdpEventConnectFailed ev) {
    try {
      BoltInternal.GlobalEventListenerBase.ConnectFailedInvoke(ev.EndPoint, ev.Token.ToToken());
    }
    finally {
      Shutdown();
    }
  }

  static void Udp_ConnectRefused(UdpEventConnectRefused ev) {
    try {
      BoltInternal.GlobalEventListenerBase.ConnectRefusedInvoke(ev.EndPoint, ev.Token.ToToken());
    }
    finally {
      Shutdown();
    }
  }

  static void Udp_ConnectRequest(UdpEventConnectRequest ev) {
    BoltInternal.GlobalEventListenerBase.ConnectRequestInvoke(ev.EndPoint, ev.Token.ToToken());
  }

  public static void AcceptConnection(UdpEndPoint endpoint, object userToken, IProtocolToken acceptToken) {
    if (!isServer) {
      BoltLog.Error("AcceptConnection can only be called on the server");
      return;
    }

    if (_config.serverConnectionAcceptMode != BoltConnectionAcceptMode.Manual) {
      BoltLog.Warn("AcceptConnection can only be called BoltConnectionAcceptMode is set to Manual");
      return;
    }

    _udpSocket.Accept(endpoint, userToken, acceptToken.ToByteArray());
  }

  public static void RefuseConnection(UdpEndPoint endpoint, IProtocolToken token) {
    if (!isServer) {
      BoltLog.Error("RefuseConnection can only be called on the server");
      return;
    }

    if (_config.serverConnectionAcceptMode != BoltConnectionAcceptMode.Manual) {
      BoltLog.Warn("RefuseConnection can only be called BoltConnectionAcceptMode is set to Manual");
      return;
    }

    _udpSocket.Refuse(endpoint, token.ToByteArray());
  }

  public static TimeSpan SendTime;
  public static TimeSpan AutoscopeTime;

  internal static void Send() {
    if (hasSocket) {
      // auto scope everything
      // Stopwatch sw;

      //sw = Stopwatch.StartNew();
      if (BoltCore._config.scopeMode == ScopeMode.Automatic) {
        var eo = _entitiesOK.GetIterator();

        while (eo.Next()) {
          if (eo.val.IsFrozen) {
            continue;
          }

          var cn = _connections.GetIterator();

          while (cn.Next()) {
            cn.val._entityChannel.CreateOnRemote(eo.val);
          }
        }
      }

      //AutoscopeTime = sw.Elapsed;

      //Debug.Log("Autoscope:" + sw.Elapsed);

      BoltPhysics.SnapshotWorld();

      //sw = Stopwatch.StartNew();
      //// switch perf counters
      if ((_frame % framesPerSecond) == 0) {
        var it = _connections.GetIterator();

        while (it.Next()) {
          it.val.SwitchPerfCounters();
        }
      }

      //Debug.Log("SwitchPerfCounters:" + sw.Elapsed);

      // send data on all connections
      {
        //sw = Stopwatch.StartNew();

        var it = _connections.GetIterator();

        while (it.Next()) {
          var modifiedSendRate = localSendRate * it.val.SendRateMultiplier;

          // if both connection and local can receive entities, use local sendrate
          if ((_frame % modifiedSendRate) == 0) {
            it.val.Send();

            if (it.val.SendRateMultiplier != 1) {
              BoltLog.Debug("Send Rate: {0} / {1}", modifiedSendRate, it.val.SendRateMultiplier);
            }
          }
        }

        //SendTime = sw.Elapsed;
        //Debug.Log("Send:" + sw.Elapsed);
      }
    }
  }

  static void UpdateUPnP() {
    if ((_frame % 60 == 0) && UPnP.Update()) {
      INatDevice device;
      IPortMapping portMapping;

      while (UPnP.NextPortStatusChange(out device, out portMapping)) {
        BoltInternal.GlobalEventListenerBase.PortMappingChangedInvoke(device, portMapping);
      }
    }
  }

  public static TimeSpan PollNetworkTime;
  public static TimeSpan InvokeRemoteSceneCallbacksTime;
  public static TimeSpan AdjustEstimatedRemoteFramesTime;
  public static TimeSpan StepNonControlledRemoteEntitiesTime;
  public static TimeSpan SimulateLocalAndControlledEntitiesTime;
  public static TimeSpan DispatchAllEventsTime;

  internal static void Poll() {
    if (hasSocket) {
      _frame += 1;

      //Stopwatch sw = null;

      BoltCore.UpdateUPnP();

      // first thing we do is to poll the network
      //sw = Stopwatch.StartNew();
      BoltCore.PollNetwork();
      //PollNetworkTime = sw.Elapsed;

      // do things!
      //sw = Stopwatch.StartNew();
      BoltCore.InvokeRemoteSceneCallbacks();
      //InvokeRemoteSceneCallbacksTime = sw.Elapsed;

      // adjust estimated frame numbers for connections
      //sw = Stopwatch.StartNew();
      BoltCore.AdjustEstimatedRemoteFrames();
      //AdjustEstimatedRemoteFramesTime = sw.Elapsed;

      // step remote events and entities which depends on remote estimated frame numbers
      //sw = Stopwatch.StartNew();
      BoltCore.StepNonControlledRemoteEntities();
      //StepNonControlledRemoteEntitiesTime = sw.Elapsed;

      // step entities which we in some way are controlling locally
      //sw = Stopwatch.StartNew();
      var iter = _entitiesOK.GetIterator();

      while (iter.Next()) {
        if (iter.val.IsFrozen) {
          continue;
        }

        if (iter.val.IsOwner || iter.val.HasPredictedControl) {
          iter.val.Simulate();
        }
      }

      //SimulateLocalAndControlledEntitiesTime = sw.Elapsed;

      // freeze all proxies
      FreezeProxies();

      //sw = Stopwatch.StartNew();
      Bolt.EventDispatcher.DispatchAllEvents();
      //DispatchAllEventsTime = sw.Elapsed;
    }
  }

  internal static void FreezeProxies() {
    var it = _entitiesOK.GetIterator();
    var freezeList = new List<Entity>();

    while (it.Next()) {
      if ((it.val.AutoFreezeProxyFrames > 0) && !it.val.IsOwner && !it.val.HasControl && (it.val.LastFrameReceived + it.val.AutoFreezeProxyFrames < BoltNetwork.frame)) {
        freezeList.Add(it.val);
      }
    }

    for (int i = 0; i < freezeList.Count; ++i) {
      freezeList[i].Freeze(true);
    }
  }

  static void InvokeRemoteSceneCallbacks() {
    if (_localSceneLoading.State == Bolt.SceneLoadState.STATE_LOADING_DONE) {
      var it = _connections.GetIterator();

      while (it.Next()) {
        var sameScene = it.val._remoteSceneLoading.Scene == _localSceneLoading.Scene;
        var loadingDone = it.val._remoteSceneLoading.State == Bolt.SceneLoadState.STATE_LOADING_DONE;

        if (sameScene && loadingDone) {
          try {
            BoltInternal.GlobalEventListenerBase.SceneLoadRemoteDoneInvoke(it.val);

            if (_localSceneLoading.Token != null) {
              BoltInternal.GlobalEventListenerBase.SceneLoadRemoteDoneInvoke(it.val, _localSceneLoading.Token);
            }
          }
          finally {
            it.val._remoteSceneLoading.State = Bolt.SceneLoadState.STATE_CALLBACK_INVOKED;
          }
        }
      }
    }
  }

  static void Udp_Connected(UdpConnection udp) {
    if (isClient) {
      Bolt.NetworkIdAllocator.Assigned(udp.ConnectionId);

      foreach (Entity eo in _entities) {
        // if we have instantiated something, this MUST have uint.MaxValue as connection id
        Assert.True(eo.NetworkId.Connection == uint.MaxValue);

        // update with our received connection id
        eo.NetworkId = new NetworkId(udp.ConnectionId, eo.NetworkId.Entity);
      }
    }

    BoltConnection cn;

    cn = new BoltConnection(udp);
    cn.AcceptToken = udp.AcceptToken.ToToken();
    cn.ConnectToken = udp.ConnectToken.ToToken();

    // put on connection list
    _connections.AddLast(cn);

    // generic connected callback
    BoltInternal.GlobalEventListenerBase.ConnectedInvoke(cn);

    // spawn entities
    if (_config.scopeMode == ScopeMode.Automatic) {
      foreach (Entity eo in _entities) {
        cn._entityChannel.CreateOnRemote(eo);
      }
    }
  }

  static void Udp_Disconnect(UdpConnection udp) {
    BoltConnection cn;
    
    cn = (BoltConnection)udp.UserToken;
    cn.DisconnectToken = udp.DisconnectToken.ToToken();

    // generic disconnected callback
    BoltInternal.GlobalEventListenerBase.DisconnectedInvoke(cn);

    if (hasSocket) {
      // cleanup                                                      
      try {
        cn.DisconnectedInternal();
      }
      catch (Exception exn) {
        BoltLog.Error(exn);
      }

      // remove from connection list
      _connections.Remove(cn);

      // if this is the client, we should shutdown all of bolt when we get disconnected
      if (cn.udpConnection.IsClient) {
        Shutdown();
      }
    }
  }

  static internal void UpdateActiveGlobalBehaviours(int index) {
#if DEBUG
    var useConsole = (_config.logTargets & BoltConfigLogTargets.Console) == BoltConfigLogTargets.Console;
    if (useConsole) {
      BoltConsole console = CreateGlobalBehaviour(typeof(BoltConsole)) as BoltConsole;

      if (console) {
        console.toggleKey = BoltRuntimeSettings.instance.consoleToggleKey;
        console.visible = BoltRuntimeSettings.instance.consoleVisibleByDefault;
      }
    }
    else {
      DeleteGlobalBehaviour(typeof(BoltConsole));
    }
#endif

    CreateGlobalBehaviour(typeof(BoltPoll));
    CreateGlobalBehaviour(typeof(BoltSend));
    CreateGlobalBehaviour(typeof(BoltSceneLoader));

    foreach (var pair in _globalBehaviours) {
      if ((pair.item0.Mode & _mode) == _mode) {
        var anyMap = ((pair.item0.Scenes == null) || (pair.item0.Scenes.Length == 0)) && ((pair.item0.Scenes == null) || (pair.item0.Scenes.Length == 0));
        var nameMatches = (index >= 0) && (pair.item0.Scenes != null) && (Array.FindIndex<string>(pair.item0.Scenes, v => Regex.IsMatch(BoltNetworkInternal.GetSceneName(index), v)) != -1);

        if (anyMap || nameMatches) {
          CreateGlobalBehaviour(pair.item1);
        }
        else {
          DeleteGlobalBehaviour(pair.item1);
        }
      }
      else {
        DeleteGlobalBehaviour(pair.item1);
      }
    }
  }

  static Component CreateGlobalBehaviour(Type t) {
    if (_globalBehaviourObject) {
      var component = _globalBehaviourObject.GetComponent(t);
      var shouldCreate = !component;

      if (shouldCreate) {
        BoltLog.Debug("Creating Global Behaviour: '{0}'", t);
        return _globalBehaviourObject.AddComponent(t);
      }
    }

    return null;
  }

  static void DeleteGlobalBehaviour(Type t) {
    if (_globalBehaviourObject) {
      var component = _globalBehaviourObject.GetComponent(t);
      var shouldDelete = !!component;

      if (shouldDelete) {
        BoltLog.Debug("Deleting Global Behaviour: '{0}'", t);
        Component.Destroy(component);
      }
    }
  }

  static void UnityLogCallback(string condition, string stackTrace, LogType type) {
    stackTrace = (stackTrace ?? "").Trim();

    switch (type) {
      case LogType.Error:
      case LogType.Assert:
      case LogType.Exception:
        BoltLog.Error(condition);

        if (stackTrace.Length > 0) {
          BoltLog.Error(stackTrace);
        }
        break;

      case LogType.Log:
        BoltLog.Info(condition);

        if (stackTrace.Length > 0) {
          BoltLog.Info(stackTrace);
        }
        break;

      case LogType.Warning:
        BoltLog.Warn(condition);

        if (stackTrace.Length > 0) {
          BoltLog.Warn(stackTrace);
        }
        break;
    }
  }

  internal static void Initialize(BoltNetworkModes mode, UdpEndPoint endpoint, BoltConfig config, UdpPlatform udpPlatform, string autoloadscene) {
    _autoloadScene = autoloadscene;

    if (!_globalControlObject) {
      _globalControlObject = new GameObject("BoltControl");
      _globalControlObject.AddComponent<ControlBehaviour>();

      GameObject.DontDestroyOnLoad(_globalControlObject);
    }

    _globalControlObject.SendMessage("QueueStart", new ControlCommandStart { Mode = mode, EndPoint = endpoint, Config = config.Clone(), Platform = udpPlatform });
  }

#if DEBUG
  static Func<float> CreatePerlinNoise() {
    var x = UnityEngine.Random.value;
    var s = Stopwatch.StartNew();
    return () => Mathf.PerlinNoise(x, (float)s.Elapsed.TotalSeconds);
  }

  static Func<float> CreateRandomNoise() {
    var r = new System.Random();
    return () => (float)r.NextDouble();
  }
#endif

  static void UdpLogWriter(uint level, string message) {
#if DEBUG
    switch (level) {
      case UdpLog.DEBUG:
      case UdpLog.TRACE:
        BoltLog.Debug(message);
        break;

      case UdpLog.INFO:
        BoltLog.Info(message);
        break;

      case UdpLog.WARN:
        BoltLog.Warn(message);
        break;

      case UdpLog.ERROR:
        BoltLog.Error(message);
        break;
    }
#endif
  }

  internal static void SceneLoadBegin(SceneLoadState state) {
    foreach(var itval in _entities) {
      if (itval.IsOwner && (itval.PersistsOnSceneLoad == false)) {
        DestroyForce(itval);
      }
    }

    // clear out scene entities
    _sceneObjects = new Dictionary<UniqueId, BoltEntity>();

    // update behaviours
    UpdateActiveGlobalBehaviours(state.Scene.Index);

    // call out to user code
    BoltInternal.GlobalEventListenerBase.SceneLoadLocalBeginInvoke(BoltNetworkInternal.GetSceneName(state.Scene.Index));

    if (state.Token != null) {
      BoltInternal.GlobalEventListenerBase.SceneLoadLocalBeginInvoke(BoltNetworkInternal.GetSceneName(state.Scene.Index), state.Token);
    }
  }

  internal static void SceneLoadDone(SceneLoadState state) {
    // switch local state
    if (state.Scene == _localSceneLoading.Scene) {
      _localSceneLoading.State = SceneLoadState.STATE_LOADING_DONE;
    }

    // 
    UpdateSceneObjectsLookup();

    // call out to user code
    BoltInternal.GlobalEventListenerBase.SceneLoadLocalDoneInvoke(BoltNetworkInternal.GetSceneName(state.Scene.Index));

    if (state.Token != null) {
      BoltInternal.GlobalEventListenerBase.SceneLoadLocalDoneInvoke(BoltNetworkInternal.GetSceneName(state.Scene.Index), state.Token);
    }
  }

  internal static void UpdateSceneObjectsLookup() {
    // grab all scene entities
    _sceneObjects =
      UE.GameObject.FindObjectsOfType(typeof(BoltEntity))
        .Cast<BoltEntity>()
        .Where(x => x.sceneGuid != UniqueId.None)
        .ToDictionary(x => x.sceneGuid);

    // how many?
    BoltLog.Debug("Found {0} Scene Objects", _sceneObjects.Count);

    // update settings
    foreach (var se in _sceneObjects.Values) {
      // attach on server
      if (isServer && (se.isAttached == false) && se._sceneObjectAutoAttach) {
        BoltEntity entity;

        entity = Attach(se.gameObject, EntityFlags.SCENE_OBJECT).GetComponent<BoltEntity>();
        entity.Entity.SceneId = se.sceneGuid;
      }
    }
  }

  internal static GameObject FindSceneObject(UniqueId uniqueId) {
    BoltEntity entity;

    if (_sceneObjects.TryGetValue(uniqueId, out entity)) {
      return entity.gameObject;
    }

    return null;
  }

  internal static UdpPacket AllocateUdpPacket() {
    return _udpSocket.PacketPool.Acquire();
  }

  internal static UdpChannelName CreateStreamChannel(string name, UdpChannelMode mode, int priority) {
    if (_udpSocket.State != UdpSocketState.Created) {
      throw new BoltException("You can only create stream channels in the Bolt.GlobalEventListener.BoltStartBegin callback.");
    }

    return _udpSocket.StreamChannelCreate(name, mode, priority);
  }

  internal static void Update() {
    var it = _entitiesOK.GetIterator();

    while (it.Next()) {
      if (it.val.IsFrozen) {
        continue;
      }

      it.val.Render();
    }
  }

  static void CreateUdpConfig(BoltConfig config) {
    var isHost = _mode == BoltNetworkModes.Host;

    // setup udpkit configuration
    _udpConfig = new UdpConfig();
    _udpConfig.PacketWindow = 512;
    _udpConfig.ConnectionTimeout = (uint)config.connectionTimeout;
    _udpConfig.ConnectRequestAttempts = (uint)config.connectionRequestAttempts;
    _udpConfig.ConnectRequestTimeout = (uint)config.connectionRequestTimeout;

#if DEBUG
    if (config.useNetworkSimulation) {
      _udpConfig.SimulatedLoss = Mathf.Clamp01(config.simulatedLoss);
      _udpConfig.SimulatedPingMin = Mathf.Max(0, (config.simulatedPingMean >> 1) - (config.simulatedPingJitter >> 1));
      _udpConfig.SimulatedPingMax = Mathf.Max(0, (config.simulatedPingMean >> 1) + (config.simulatedPingJitter >> 1));

      switch (config.simulatedRandomFunction) {
        case BoltRandomFunction.PerlinNoise: _udpConfig.NoiseFunction = CreatePerlinNoise(); break;
        case BoltRandomFunction.SystemRandom: _udpConfig.NoiseFunction = CreateRandomNoise(); break;
      }
    }
#endif

    _udpConfig.MasterServerAutoDisconnect = BoltRuntimeSettings.instance.masterServerAutoDisconnect;
    _udpConfig.ConnectionLimit = isHost ? config.serverConnectionLimit : 0;
    _udpConfig.AllowIncommingConnections = isHost;
    _udpConfig.AutoAcceptIncommingConnections = isHost && (_config.serverConnectionAcceptMode == BoltConnectionAcceptMode.Auto);
    _udpConfig.PingTimeout = (uint)(localSendRate * 1.5f * frameDeltaTime * 1000f);
    _udpConfig.PacketDatagramSize = Mathf.Clamp(_config.packetSize, 1024, 4096);
  }


  static void CreateBoltBehaviourObject() {
    // create the gflobal 'Bolt' unity object
    if (_globalBehaviourObject) {
      GameObject.Destroy(_globalBehaviourObject);
    }

    _globalBehaviours = BoltNetworkInternal.GetGlobalBehaviourTypes();
    _globalBehaviourObject = new GameObject("BoltBehaviours");

    GameObject.DontDestroyOnLoad(_globalBehaviourObject);
  }

  static void ResetIdAllocator(BoltNetworkModes mode) {
    if (mode == BoltNetworkModes.Host) {
      NetworkIdAllocator.Reset(1U);
    }
    else {
      NetworkIdAllocator.Reset(uint.MaxValue);
    }
  }

  internal static void BeginStart(ControlCommandStart cmd) {
    if (BoltNetwork.isRunning) {
      cmd.State = ControlState.Failed;

      // make sure we don't wait for this
      cmd.FinishedEvent.Set();

      // 
      throw new BoltException("Bolt is already running, you must call BoltLauncher.Shutdown() before starting a new instance of Bolt.");
    }

    // done!
    _mode = cmd.Mode;
    _config = cmd.Config;
    _udpPlatform = cmd.Platform;
    _canReceiveEntities = true;

    // reset id allocator
    ResetIdAllocator(_mode);

    // clear everything in console
    BoltConsole.Clear();

    // setup debug info display
    if (BoltRuntimeSettings.instance.showDebugInfo) {
      DebugInfo.SetupAndShow();
    }

    // setup logging
    BoltLog.Setup(_mode, _config.logTargets);

    // tell user we're starting
    BoltLog.Debug("Bolt starting with a simulation rate of {0} steps per second", _config.framesPerSecond);

    // set frametime
    Time.fixedDeltaTime = 1f / (float)_config.framesPerSecond;

    // create prefab cache
    PrefabDatabase.BuildCache();

    // set udpkits log writer
    UdpLog.SetWriter(UdpLogWriter);

    // create the gameobject that holds all bolt global behaviours, etc.
    CreateBoltBehaviourObject();

    // call to generated code so it knows we're starting
    BoltNetworkInternal.EnvironmentSetup();

    // create updp config
    CreateUdpConfig(_config);

    // setup default local scene load state
    _localSceneLoading = SceneLoadState.DefaultLocal();

    // create and start socket
    _udpSocket = new UdpSocket(Zeus.GameGuid, _udpPlatform, _udpConfig);

    // init all global behaviours
    UpdateActiveGlobalBehaviours(-1);

    // invoke started
    BoltInternal.GlobalEventListenerBase.BoltStartBeginInvoke();

    //  start socket
    _udpSocket.Start(cmd.EndPoint, cmd.FinishedEvent, ((_mode == BoltNetworkModes.Host) ? UdpSocketMode.Host : UdpSocketMode.Client));

    // should we automatically connect to default zeus?
    if (BoltRuntimeSettings.instance.masterServerAutoConnect && (Zeus.GameGuid != Guid.Empty)) {
      Zeus.Connect();
    }
  }

  internal static void ShutdownImmediate() {
    if (!BoltNetwork.isRunning) {
      return;
    }

    // disconnect from zeus
    Zeus.Disconnect();

    // disable upnp
    UPnP.Disable(false);

    // 
    _mode = BoltNetworkModes.None;

    // destroy all entities
    foreach (Bolt.Entity entity in _entities.ToArray()) {
      try {
        DestroyForce(entity);
      }
      catch {

      }
    }

    _entitiesFZ.Clear();
    _entitiesOK.Clear();
    _connections.Clear();
    _globalEventDispatcher.Clear();
    _globalBehaviours.Clear();

    if (_globalBehaviourObject) {
      GameObject.Destroy(_globalBehaviourObject);
    }

    BoltNetworkInternal.EnvironmentReset();

    // set a specificl writer for this
    UdpLog.SetWriter((i, m) => UnityEngine.Debug.Log(m));

    // begin closing socket
    _udpSocket.Close(null);

    // clear socket
    _udpSocket = null;

    Factory.UnregisterAll();
    BoltLog.RemoveAll();
    BoltConsole.Clear();
    DebugInfo.Hide();
  }

  internal static void BeginShutdown(ControlCommandShutdown cmd) {
    if (!BoltNetwork.isRunning) {
      cmd.State = ControlState.Failed;
      cmd.FinishedEvent.Set();

      throw new BoltException("Bolt is not running so it can't be shutdown");
    }

    // notify user code
    BoltInternal.GlobalEventListenerBase.BoltShutdownBeginInvoke(cmd.Callbacks.Add);

    // disconnect from zeus
    Zeus.Disconnect();

    // disable upnp
    UPnP.Disable(false);

    // 
    _mode = BoltNetworkModes.Shutdown;

    // destroy all entities
    foreach (Bolt.Entity entity in _entities.ToArray()) {
      try {
        DestroyForce(entity);
      }
      catch { }
    }

    _entitiesFZ.Clear();
    _entitiesOK.Clear();
    _connections.Clear();
    _globalEventDispatcher.Clear();
    _globalBehaviours.Clear();

    if (_globalBehaviourObject) {
      // disables the immediate shutdown bolt does in the editor and OnApplicationQuit
      _globalBehaviourObject.GetComponent<BoltPoll>().AllowImmediateShutdown = false;

      // destroy everything!
      GameObject.Destroy(_globalBehaviourObject);
    }

    // reset environment stuff (we can probably remove this)
    BoltNetworkInternal.EnvironmentReset();

    // set a specificl writer for this
    UdpLog.SetWriter((i, m) => UnityEngine.Debug.Log(m));

    Debug.Log(_udpSocket);

    // begin closing socket
    _udpSocket.Close(cmd.FinishedEvent);

    // clear socket
    _udpSocket = null;

    Factory.UnregisterAll();
    BoltLog.RemoveAll();
    BoltConsole.Clear();
    DebugInfo.Hide();
  }
}
