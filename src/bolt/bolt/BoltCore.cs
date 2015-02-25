using Bolt;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UdpKit;
using UnityEngine;
using UE = UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;
using System.Text.RegularExpressions;
using System.Threading;

/// <summary>
/// The network mode of this bolt simulation (i.e. client or server)
/// </summary>
public enum BoltNetworkModes {
  None = 0,
  Server = 1,
  Client = 2,
}

internal static class BoltCore {
  internal static UdpSocket _udpSocket;
  internal static UdpPlatform _udpPlatform;

  static internal Stopwatch _timer = new Stopwatch();
  static internal SceneLoadState _localSceneLoading;

  static internal bool _canReceiveEntities = true;
  static internal IPrefabPool PrefabPool = new DefaultPrefabPool();
  static internal IEventFilter EventFilter = new DefaultEventFilter();

  static internal int _frame = 0;
  static internal BoltNetworkModes _mode = BoltNetworkModes.None;

  static internal BoltConfig _config = null;
  static internal UdpConfig _udpConfig = null;

  static internal BoltDoubleList<Entity> _entities = new BoltDoubleList<Entity>();

  static internal BoltDoubleList<BoltConnection> _connections = new BoltDoubleList<BoltConnection>();
  static internal Bolt.EventDispatcher _globalEventDispatcher = new Bolt.EventDispatcher();
  static internal Dictionary<UniqueId, BoltEntity> _sceneObjects = new Dictionary<UniqueId, BoltEntity>(UniqueId.EqualityComparer.Instance);

  static internal GameObject _globalBehaviourObject = null;
  static internal List<STuple<BoltGlobalBehaviourAttribute, Type>> _globalBehaviours = new List<STuple<BoltGlobalBehaviourAttribute, Type>>();

  public static bool isDebugMode {
#if DEBUG
    get { return true; }
#else
    get { return false; }
#endif
  }

  [Obsolete("This property is not used anymore, and is always null")]
  public static GameObject userObject {
    get { return null; }
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
    get { return _mode == BoltNetworkModes.None ? 0 : (isServer ? _frame : server.remoteFrame); }
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
    get { return hasSocket && _mode == BoltNetworkModes.Server; }
  }

  internal static int localSendRate {
    get {
      switch (_mode) {
        case BoltNetworkModes.Server: return _config.serverSendRate;
        case BoltNetworkModes.Client: return _config.clientSendRate;
        default: return -1;
      }
    }
  }

  internal static int remoteSendRate {
    get {
      switch (_mode) {
        case BoltNetworkModes.Server: return _config.clientSendRate;
        case BoltNetworkModes.Client: return _config.serverSendRate;
        default: return -1;
      }
    }
  }

  internal static int localInterpolationDelay {
    get {
      switch (_mode) {
        case BoltNetworkModes.Server: return _config.serverDejitterDelay;
        case BoltNetworkModes.Client: return _config.clientDejitterDelay;
        default: return -1;
      }
    }
  }

  internal static int localInterpolationDelayMin {
    get {
      switch (_mode) {
        case BoltNetworkModes.Server: return _config.serverDejitterDelayMin;
        case BoltNetworkModes.Client: return _config.clientDejitterDelayMin;
        default: return -1;
      }
    }
  }

  internal static int localInterpolationDelayMax {
    get {
      switch (_mode) {
        case BoltNetworkModes.Server: return _config.serverDejitterDelayMax;
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
    return Attach(gameObject, Factory.GetFactory(be.serializerGuid).TypeId, flags, null);
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

    var it = _entities.GetIterator();

    while (it.Next()) {
      if (it.val.NetworkId == id) {
        return it.val;
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

  public static ManualResetEvent Shutdown() {
    if (_udpSocket != null && _mode != BoltNetworkModes.None) {
      // log that we are shutting down
      BoltLog.Info("Shutdown");

      // notify user code
      BoltInternal.GlobalEventListenerBase.BoltShutdownInvoke();

      // disconnect from zeus
      Zeus.Disconnect();

      // disable upnp
      UPnP.Disable(false);

      // 
      _mode = BoltNetworkModes.None;

      // destroy all entities
      foreach (Bolt.Entity entity in _entities.ToArray()) {
        DestroyForce(entity);
      }

      _entities.Clear();
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
      var resetEvent = _udpSocket.Close();

      // clear socket
      _udpSocket = null;

      Factory.UnregisterAll();
      BoltLog.RemoveAll();
      BoltConsole.Clear();
      DebugInfo.Hide();

      return resetEvent;
    }

    return new ManualResetEvent(true);
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
        case UdpEventType.SocketStartupFailed:
          BoltInternal.GlobalEventListenerBase.BoltStartFailedInvoke();
          Shutdown();
          break;

        case UdpEventType.SocketStartupDone:
          BoltInternal.GlobalEventListenerBase.BoltStartedInvoke();
          break;

        case UdpEventType.Connected:
          HandleConnected(ev.Connection);
          break;

        case UdpEventType.Disconnected:
          HandleDisconnected(ev.Connection.GetBoltConnection(), ev.DisconnectToken);
          break;

        case UdpEventType.ConnectRequest:
          HandleConnectRequest(ev.EndPoint, ev.ConnectToken);
          break;

        case UdpEventType.ConnectFailed:
          HandleConnectFailed(ev.EndPoint);
          break;

        case UdpEventType.ConnectRefused:
          HandleConnectRefused(ev.EndPoint, ev.RefusedToken);
          break;

        case UdpEventType.ConnectAttempt:
          BoltInternal.GlobalEventListenerBase.ConnectAttemptInvoke(ev.EndPoint);
          break;

        case UdpEventType.PacketLost:
          using (var packet = (Packet)ev.Packet.UserToken) {
            ev.Connection.GetBoltConnection().PacketLost(packet);
          }
          break;

        case UdpEventType.PacketDelivered:
          using (var packet = (Packet)ev.Packet.UserToken) {
            ev.Connection.GetBoltConnection().PacketDelivered(packet);
          }
          break;

        case UdpEventType.PacketReceived:
          ev.Connection.GetBoltConnection().PacketReceived(ev.Packet);
          break;

        case UdpEventType.StreamDataReceived:
          BoltInternal.GlobalEventListenerBase.StreamDataReceivedInvoke(
            ev.Connection.GetBoltConnection(),
            ev.StreamData
          );
          break;

        // SESSION

        case UdpEventType.SessionListUpdated:
          // store session list
          BoltNetwork._sessionList = ev.SessionList;

          // notify user
          BoltInternal.GlobalEventListenerBase.SessionListUpdatedInvoke(BoltNetwork._sessionList);
          break;

        case UdpEventType.SessionConnectFailed:
          BoltInternal.GlobalEventListenerBase.SessionConnectFailedInvoke(ev.Session);
          break;

        // MASTER SERVER

        case UdpEventType.MasterServerConnected:
          BoltInternal.GlobalEventListenerBase.ZeusConnectedInvoke(ev.EndPoint);
          break;

        case UdpEventType.MasterServerDisconnected:
          BoltInternal.GlobalEventListenerBase.ZeusDisconnectedInvoke(ev.EndPoint);
          break;

        case UdpEventType.MasterServerConnectFailed:
          BoltInternal.GlobalEventListenerBase.ZeusConnectFailedInvoke(ev.EndPoint);
          break;

        case UdpEventType.MasterServerNatProbeResult:
          BoltInternal.GlobalEventListenerBase.ZeusNatProbeResultInvoke(ev.NatFeatures);
          break;
      }
    }
  }

  static void HandleConnectFailed(UdpEndPoint endpoint) {
    try {
      BoltInternal.GlobalEventListenerBase.ConnectFailedInvoke(endpoint);
    }
    finally {
      Shutdown();
    }
  }

  static void HandleConnectRefused(UdpEndPoint endpoint, byte[] token) {
    try {
      BoltInternal.GlobalEventListenerBase.ConnectRefusedInvoke(endpoint);
      BoltInternal.GlobalEventListenerBase.ConnectRefusedInvoke(endpoint, token.ToToken());
    }
    finally {
      Shutdown();
    }
  }

  static void HandleConnectRequest(UdpEndPoint endpoint, byte[] token) {
    BoltInternal.GlobalEventListenerBase.ConnectRequestInvoke(endpoint);
    BoltInternal.GlobalEventListenerBase.ConnectRequestInvoke(endpoint, token.ToToken());
  }

  public static void AcceptConnection(UdpEndPoint endpoint, object userToken, IProtocolToken acceptToken, IProtocolToken connectToken) {
    if (!isServer) {
      BoltLog.Error("AcceptConnection can only be called on the server");
      return;
    }

    if (_config.serverConnectionAcceptMode != BoltConnectionAcceptMode.Manual) {
      BoltLog.Warn("AcceptConnection can only be called BoltConnectionAcceptMode is set to Manual");
      return;
    }

    _udpSocket.Accept(endpoint, userToken, acceptToken.ToByteArray(), connectToken.ToByteArray());
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

  internal static void Send() {
    if (hasSocket) {
      // auto scope everything
      if (BoltCore._config.scopeMode == ScopeMode.Automatic) {
        var eo = _entities.GetIterator();

        while (eo.Next()) {
          var cn = _connections.GetIterator();

          while (cn.Next()) {
            cn.val._entityChannel.CreateOnRemote(eo.val);
          }
        }
      }

      BoltPhysics.SnapshotWorld();

      // switch perf counters
      if ((_frame % framesPerSecond) == 0) {
        var it = _connections.GetIterator();

        while (it.Next()) {
          it.val.SwitchPerfCounters();
        }
      }

      // send data on all connections
      {
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

  internal static void Poll() {
    if (hasSocket) {
      _frame += 1;

      BoltCore.UpdateUPnP();

      // first thing we do is to poll the network
      BoltCore.PollNetwork();

      // do things!
      BoltCore.InvokeRemoteSceneCallbacks();

      // adjust estimated frame numbers for connections
      BoltCore.AdjustEstimatedRemoteFrames();

      // step remote events and entities which depends on remote estimated frame numbers
      BoltCore.StepNonControlledRemoteEntities();

      // step entities which we in some way are controlling locally
      var iter = _entities.GetIterator();

      while (iter.Next()) {
        if (!iter.val.IsFrozen && (iter.val.IsOwner || iter.val.HasPredictedControl)) {
          iter.val.Simulate();
        }
      }

      // freeze all proxies
      FreezeProxies();

      Bolt.EventDispatcher.DispatchAllEvents();
    }
  }

  internal static void FreezeProxies() {
    var it = _entities.GetIterator();
    var freezeList = new List<Entity>();

    while (it.Next()) {
      if (!it.val.IsOwner && !it.val.HasControl && (it.val.AutoFreezeProxyFrames > 0) && (it.val.LastFrameReceived + it.val.AutoFreezeProxyFrames < BoltNetwork.frame)) {
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

  static void HandleConnected(UdpConnection udp) {
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
    BoltInternal.GlobalEventListenerBase.ConnectedInvoke(cn, cn.AcceptToken);
    BoltInternal.GlobalEventListenerBase.ConnectedInvoke(cn, cn.AcceptToken, cn.ConnectToken);

    // spawn entities
    if (_config.scopeMode == ScopeMode.Automatic) {
      foreach (Entity eo in _entities) {
        cn._entityChannel.CreateOnRemote(eo);
      }
    }
  }

  static void HandleDisconnected(BoltConnection cn, byte[] token) {
    // generic disconnected callback
    BoltInternal.GlobalEventListenerBase.DisconnectedInvoke(cn);
    BoltInternal.GlobalEventListenerBase.DisconnectedInvoke(cn, token.ToToken());

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

  internal static void Initialize(BoltNetworkModes mode, UdpEndPoint endpoint, BoltConfig config, UdpPlatform udpPlatform) {
    _udpPlatform = udpPlatform;

    BoltConsole.Clear();

    // close any existing socket
    Shutdown();

    var isServer = mode == BoltNetworkModes.Server;
    var isClient = mode == BoltNetworkModes.Client;

    if (isServer) {
      NetworkIdAllocator.Reset(1U);
    }
    else {
      NetworkIdAllocator.Reset(uint.MaxValue);
    }

    PrefabDatabase.BuildCache();

    if (BoltRuntimeSettings.instance.showDebugInfo) {
      DebugInfo.ignoreList = new HashSet<NetworkId>();
      DebugInfo.Show();
    }

#if DEBUG
    if (BoltRuntimeSettings.instance.logUncaughtExceptions) {
      UE.Application.RegisterLogCallbackThreaded(UnityLogCallback);
    }
#endif


#if DEBUG
    // init loggers
    var fileLog = (config.logTargets & BoltConfigLogTargets.File) == BoltConfigLogTargets.File;
    var unityLog = (config.logTargets & BoltConfigLogTargets.Unity) == BoltConfigLogTargets.Unity;
    var consoleLog = (config.logTargets & BoltConfigLogTargets.Console) == BoltConfigLogTargets.Console;
    var systemOutLog = (config.logTargets & BoltConfigLogTargets.SystemOut) == BoltConfigLogTargets.SystemOut;

    if (unityLog && (BoltRuntimeSettings.instance.logUncaughtExceptions == false)) { BoltLog.Add(new BoltLog.Unity()); }
    if (consoleLog) { BoltLog.Add(new BoltLog.Console()); }
    if (systemOutLog) { BoltLog.Add(new BoltLog.SystemOut()); }
    if (fileLog) {
      switch (Application.platform) {
        case RuntimePlatform.OSXEditor:
        case RuntimePlatform.WindowsEditor:
        case RuntimePlatform.WindowsPlayer:
        case RuntimePlatform.OSXPlayer:
          BoltLog.Add(new BoltLog.File(mode == BoltNetworkModes.Server));
          break;
      }
    }
#endif

    // set config
    _config = config;
    _canReceiveEntities = true;

    // set frametime
    Time.fixedDeltaTime = 1f / (float)config.framesPerSecond;

    // set udpkits log writer
    UdpLog.SetWriter(UdpLogWriter);

    // :)
    BoltLog.Debug("Starting at {0} fps ({1} fixed frame delta)", config.framesPerSecond, Time.fixedDeltaTime);

    // create the gflobal 'Bolt' unity object
    if (_globalBehaviourObject) {
      GameObject.Destroy(_globalBehaviourObject);
    }

    _globalBehaviours = BoltNetworkInternal.GetGlobalBehaviourTypes();
    _globalBehaviourObject = new GameObject("Bolt");

    GameObject.DontDestroyOnLoad(_globalBehaviourObject);

    // verify all handlers are unregistered
    Assert.True(Factory.IsEmpty);

    // setup autogen and mode
    _mode = mode;

    BoltNetworkInternal.EnvironmentSetup();

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

    _udpConfig.ConnectionLimit = isServer ? config.serverConnectionLimit : 0;
    _udpConfig.AllowIncommingConnections = isServer;
    _udpConfig.AutoAcceptIncommingConnections = isServer && (_config.serverConnectionAcceptMode == BoltConnectionAcceptMode.Auto);
    _udpConfig.PingTimeout = (uint)(localSendRate * 1.5f * frameDeltaTime * 1000f);
    _udpConfig.PacketDatagramSize = Mathf.Clamp(_config.packetSize, 1024, 4096);

    // create and start socket
    _localSceneLoading = SceneLoadState.DefaultLocal();

    Guid gameGuid = new Guid();

    try {
      gameGuid = new Guid(BoltRuntimeSettings.instance.masterServerGameId);
    }
    catch {
      gameGuid = new Guid();
      BoltLog.Error("Could not parse game id, you will not be able to connect to the Zeus server");
    }

    // create udp socket
    _udpSocket = new UdpSocket(gameGuid, udpPlatform, _udpConfig);

    // init all global behaviours
    UpdateActiveGlobalBehaviours(-1);

    // have to register channels BEFORE the socket starts
    BoltInternal.GlobalEventListenerBase.RegisterStreamChannelsInvoke();
    BoltInternal.GlobalEventListenerBase.BoltStartPendingInvoke();

    // 
    _udpSocket.Start(endpoint, (isServer ? UdpSocketMode.Host : UdpSocketMode.Client));

    if (BoltRuntimeSettings.instance.masterServerAutoConnect && (gameGuid != Guid.Empty)) {
      UdpEndPoint zeusEndPoint = new UdpEndPoint();

      try {
        zeusEndPoint = UdpEndPoint.Parse(BoltRuntimeSettings.instance.masterServerEndPoint);
      }
      catch {
        zeusEndPoint = new UdpEndPoint();
        BoltLog.Error("Could not parse Zeus server endpoint for automatic connection");
      }

      if (zeusEndPoint != UdpEndPoint.Any) {
        Zeus.Connect(zeusEndPoint);
      }
    }
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
    var it = _entities.GetIterator();

    while (it.Next()) {
      if (it.val.IsOwner && (it.val.PersistsOnSceneLoad == false)) {
        DestroyForce(it.val);
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
      throw new BoltException("You can only create stream channels in the Bolt.GlobalEventListener.RegisterStreamChannels callback.");
    }

    return _udpSocket.StreamChannelCreate(name, mode, priority);
  }

  //internal static UdpStreamData CreateStreamData(byte[] data) {
  //  return _udpSocket.StreamDataCreate(data);
  //}

  //internal static UdpStreamData FindStreamData(UdpDataKey data) {
  //  return _udpSocket.StreamDataFind(data);
  //}

  internal static void Update() {
    var it = _entities.GetIterator();

    while (it.Next()) {
      it.val.Render();
    }
  }

}
