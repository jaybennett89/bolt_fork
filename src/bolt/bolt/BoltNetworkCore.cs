using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UdpKit;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public enum BoltNetworkModes {
  None = 0,
  Server = 1,
  Client = 2,
}

internal static class BoltCore {
  static UdpSocket _udpSocket;
  static internal SceneLoadState _mapLoadState;

  static internal uint _uid;
  static internal uint _uidEntityCounter;

  static internal int _frame = 0;
  static internal byte[] _userAssemblyHash = null;
  static internal BoltNetworkModes _mode = BoltNetworkModes.None;
  static internal IBoltNetwork _autogen = null;

  static internal BoltConfig _config = null;
  static internal UdpConfig _udpConfig = null;

  static internal BoltDoubleList<BoltConnection> _connections = new BoltDoubleList<BoltConnection>();
  static internal BoltDoubleList<BoltEntityProxy> _proxies = new BoltDoubleList<BoltEntityProxy>();
  static internal BoltDoubleList<BoltEntity> _entities = new BoltDoubleList<BoltEntity>();
  static internal BoltEventDispatcher _globalEventDispatcher = new BoltEventDispatcher();

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

  public static Func<int, Transform> resolveTransform = BoltOrigin.ResolveTransform;
  public static Func<Transform, int> resolveTransformId = BoltOrigin.ResolveTransformId;


  public static Action ShutdownComplete;

  public static GameObject[] prefabs {
    get { return BoltRuntimeSettings.prefabs; }
  }

  public static bool isUnityPro {
    get { return _autogen.isUnityPro; }
  }

  public static string loadedMap {
    get { return _mapLoadState.scene.name; }
  }

  public static byte[] userAssemblyHash {
    get { return _userAssemblyHash; }
  }

  public static GameObject globalObject {
    get { return _globalBehaviourObject; }
  }

  public static IEnumerable<BoltEntity> entities {
    get { return _entities; }
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

  public static bool isClient {
    get { return hasSocket && _mode == BoltNetworkModes.Client; }
  }

  public static bool isServer {
    get { return hasSocket && _mode == BoltNetworkModes.Server; }
  }

  public static byte[] GetUserAssemblyHash() {
    return BoltRuntimeReflection.GetUserAssemblyHash();
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

  internal static int localSendRateBits {
    get { return BoltMath.Hibit((uint)localSendRate); }
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

  internal static int remoteSendRateBits {
    get { return BoltMath.Hibit((uint)remoteSendRate); }
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

  public static void InitializeServer(UdpEndPoint endpoint, IBoltNetwork autogen, BoltConfig config) {
    Initialize(BoltNetworkModes.Server, endpoint, autogen, config);
  }

  public static void InitializeClient(UdpEndPoint endpoint, IBoltNetwork autogen, BoltConfig config) {
    Initialize(BoltNetworkModes.Client, endpoint, autogen, config);
  }

  public static void Destroy(BoltEntity entity) {
    entity.Detach();
    _destroy(entity.gameObject);
  }

  public static void Destroy(GameObject go) {
    BoltEntity entity = go.GetComponent<BoltEntity>();

    if (entity) {
      Destroy(entity);
    }
    else {
      BoltLog.Error("Can only destroy gameobjects with an BoltEntity component through BoltNetwork.Destroy");
    }
  }

  public static void Detach(BoltEntity entity) {
    entity.Detach();
  }

  public static BoltEntity Instantiate(GameObject prefab) {
    return Instantiate(prefab, Vector3.zero, Quaternion.identity);
  }

  public static BoltEntity Instantiate(GameObject prefab, Vector3 position, Quaternion rotation) {
    prefab.GetComponent<BoltEntity>()._sceneObject = false;
    GameObject go = _instantiate(prefab, position, rotation);
    BoltEntity en = go.GetComponent<BoltEntity>();

    if (isClient && (en._allowInstantiateOnClient == false)) {
      throw new BoltException("This prefab is not allowed to be instantiated on clients");
    }

    return Attach(en, null, Bits.zero, GenerateUniqueId());
  }

  public static BoltEntity Attach(BoltEntity entity) {
    entity.Attach(null, Bits.zero, GenerateUniqueId());
    return entity;
  }

  public static BoltUniqueId GenerateUniqueId() {
    BoltUniqueId id = new BoltUniqueId();

    if (_config.globalUniqueIds) {
      id = new BoltUniqueId(_uid, ++_uidEntityCounter);
    }

    return id;
  }

  internal static BoltEntity Attach(BoltEntity entity, BoltConnection source, Bits flags, BoltUniqueId uniqueId) {
    entity.enabled = true;
    entity.Attach(source, flags, uniqueId);

    return entity;
  }

  public static void DontDestroyOnMapLoad(BoltEntity entity) {
    entity._flags |= BoltEntity.FLAG_PERSIST_ON_MAP_LOAD;
  }

  public static GameObject FindPrefab(string name) {
    return BoltRuntimeSettings.FindPrefab(name);
  }

  public static void LoadMap(string name) {
    if (isServer == false) {
      BoltLog.Error("only server can initiate a map load");
      return;
    }

    LoadMapInternal(new Scene(name, _mapLoadState.scene.token + 1));
  }


  internal static void LoadMapInternal(Scene map) {
    foreach (BoltEntity entity in entities) {
      // destroy entities which we are in control of and which are not labelled as proxies
      if (entity._flags & BoltEntity.FLAG_IS_PROXY) { continue; }
      if (entity._flags & BoltEntity.FLAG_PERSIST_ON_MAP_LOAD) { continue; }

      // pop!
      Destroy(entity);
    }

    if (_mapLoadState.scene != map) {
      _mapLoadState = _mapLoadState.BeginLoad(map);

      // start loading
      BoltSceneLoader.Enqueue(_mapLoadState.scene);
    }
  }

  public static void Shutdown() {
    if (_udpSocket != null && _mode != BoltNetworkModes.None) {
      BoltLog.Info("shutting down");

      try {
        // 
        _mode = BoltNetworkModes.None;
        _uid = 0;
        _mapLoadState = new SceneLoadState();

        // disconnect from everywhere
        foreach (BoltConnection connection in _connections) {
          connection.Disconnect();
        }

        foreach (BoltEntity entity in entities.ToArray()) {
          Destroy(entity);
        }

        _proxies.Clear();
        _entities.Clear();
        _connections.Clear();

        foreach (var callback in _globalEventDispatcher._targets.ToArray()) {
          if (callback is BoltCallbacksBase && ((BoltCallbacksBase)callback).PersistBetweenStartupAndShutdown()) {
            continue;
          }

          _globalEventDispatcher._targets.Remove(callback);
        }

        _globalBehaviours.Clear();

        if (_globalBehaviourObject) {
          GameObject.Destroy(_globalBehaviourObject);
        }

        _autogen.Reset();
        _autogen = null;

        _udpSocket.Close();
        _udpSocket = null;

        BoltFactory.UnregisterAll();
        BoltLog.RemoveAll();

      }
      finally {
        if (ShutdownComplete != null) {
          ShutdownComplete();
        }
      }
    }
  }

  public static UdpSession[] GetSessions() {
    return _udpSocket.GetSessions();
  }

  public static void Connect(UdpEndPoint endpoint) {
    Connect(endpoint, null);
  }

  public static void Connect(UdpEndPoint endpoint, byte[] token) {
    if (server != null) {
      BoltLog.Error("You must disconnect from the current server first");
      return;
    }

    // stop broadcasting
    DisableLanBroadcast();

    // connect
    _udpSocket.Connect(endpoint, token);
  }

  public static void Raise(IBoltEvent evnt) {
    BoltEventBase.Invoke((BoltEventBase)evnt);
  }

  public static void Raise(IBoltEvent evnt, IEnumerable connections) {
    BoltEventBase.Invoke((BoltEventBase)evnt, connections);
  }

  public static void Raise(IBoltEvent evnt, params BoltConnection[] connections) {
    BoltEventBase.Invoke((BoltEventBase)evnt, connections);
  }

  public static void SetSessionData(string serverName, string userData) {
    if (BoltCore.isServer == false) {
      BoltLog.Error("Only the server can call SetSessionData");
      return;
    }

    _udpSocket.SetSessionData(serverName, userData);
  }

  public static void EnableLanBroadcast(UdpEndPoint endpoint) {
    if (endpoint.Address == UdpIPv4Address.Any || endpoint.Port == 0) {
      BoltLog.Error("incorrect broadcast endpoint: {0}", endpoint);
    }
    else {
      _udpSocket.EnableLanBroadcast(endpoint, isServer);
    }
  }

  public static void DisableLanBroadcast() {
    _udpSocket.DisableLanBroadcast();
  }

  static void StepRemoteFrames() {
    BoltConnection cn;
    BoltIterator<BoltConnection> cnIter;

    if (hasSocket) {
      cn = null;
      cnIter = _connections.GetIterator();

      while (cnIter.Next(out cn)) {
        cn.AdjustRemoteFrame();
      }

      bool retry;

      do {
        retry = false;

        cn = null;
        cnIter = _connections.GetIterator();

        while (cnIter.Next(out cn)) {
          if (cn.StepRemoteFrame()) {
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
        case UdpEventType.Connected:
          HandleConnected(ev.Connection);
          break;

        case UdpEventType.Disconnected:
          HandleDisconnected(ev.Connection.GetBoltConnection());
          break;

        case UdpEventType.ConnectRequest:
          BoltCallbacksBase.ConnectRequestInvoke(ev.EndPoint, ev.Object0 as byte[]);
          break;

        case UdpEventType.ConnectFailed:
          BoltCallbacksBase.ConnectFailedInvoke(ev.EndPoint);
          break;

        case UdpEventType.ConnectRefused:
          BoltCallbacksBase.ConnectRefusedInvoke(ev.EndPoint);
          break;

        case UdpEventType.ObjectSent:
          ev.Connection.GetBoltConnection().PacketSent((BoltPacket)ev.Object0);
          break;

        case UdpEventType.ObjectReceived:
          using ((BoltPacket)ev.Object0) {
            ev.Connection.GetBoltConnection().PacketReceived((BoltPacket)ev.Object0);
          }
          break;

        case UdpEventType.ObjectDelivered:
          using ((BoltPacket)ev.Object0) {
            ev.Connection.GetBoltConnection().PacketDelivered((BoltPacket)ev.Object0);
          }
          break;

        case UdpEventType.ObjectLost:
        case UdpEventType.ObjectRejected:
        case UdpEventType.ObjectSendFailed:
          using ((BoltPacket)ev.Object0) {
            ev.Connection.GetBoltConnection().PacketLost((BoltPacket)ev.Object0);
          }
          break;
      }
    }
  }

  public static void AcceptConnection(UdpEndPoint endpoint) {
    AcceptConnection(endpoint, null);
  }

  public static void AcceptConnection(UdpEndPoint endpoint, object userToken) {
    if (!isServer) {
      BoltLog.Error("AcceptConnection: can only be called on the server");
      return;
    }

    if (_config.serverConnectionAcceptMode != BoltConnectionAcceptMode.Manual) {
      BoltLog.Warn("AcceptConnection: can only be called if BoltConnectionAcceptMode is set to Manual");
      return;
    }

    _udpSocket.Accept(endpoint, userToken);
  }

  public static void RefuseConnection(UdpEndPoint endpoint) {
    if (!isServer) {
      BoltLog.Error("RefuseConnection: can only be called on the server");
      return;
    }

    if (_config.serverConnectionAcceptMode != BoltConnectionAcceptMode.Manual) {
      BoltLog.Warn("RefuseConnection: can only be called if BoltConnectionAcceptMode is set to Manual");
      return;
    }

    _udpSocket.Refuse(endpoint);
  }

  public static bool HasEntity(BoltUniqueId id) {
    var it = _entities.GetIterator();

    while (it.Next()) {
      if (it.val._uniqueId == id) {
        return true;
      }
    }

    return false;
  }

  public static BoltEntity FindEntity(BoltUniqueId id) {
    if (_config.globalUniqueIds == false) {
      throw new BoltException("can only call 'FindEntity(BoltUniqueId id)' if the 'Use Globally Unique Ids' options has been turned on");
    }

    var it = _entities.GetIterator();

    while (it.Next()) {
      if (it.val._uniqueId == id) {
        return it.val;
      }
    }

    return null;
  }

  internal static void Send() {
    if (hasSocket) {
      BoltPhysics.SnapshotWorld();

      if (_frame % framesPerSecond == 0) {
        var it = _connections.GetIterator();

        while (it.Next()) {
          it.val.SwitchPerfCounters();
        }
      }

      if ((_frame % localSendRate) == 0) {
        // update the scope for all connections/entities
        BoltCore.UpdateScope();

        // copy all bitmasks from entities to proxies
        BoltEntityProxy proxy = null;
        var proxyIter = _proxies.GetIterator();

        while (proxyIter.Next(out proxy)) {
          if (proxy.entity) {
            if (proxy.entity.IsControlledBy(proxy.connection)) {
              proxy.mask |= (proxy.entity._mask & proxy.entity.boltSerializer.controllerMask);
            }
            else {
              proxy.mask |= (proxy.entity._mask & proxy.entity.boltSerializer.proxyMask);
            }
          }
        }

        // clear all masks on entities
        BoltEntity entity = null;
        var entityIter = _entities.GetIterator();

        while (entityIter.Next(out entity)) {
          entity._mask = Bits.zero;
        }

        // send out updates to all connections
        BoltConnection cn;
        var cnIter = _connections.GetIterator();

        while (cnIter.Next(out cn)) {
          cn.Send();
        }
      }
    }
  }

  internal static void FixedUpdate() {
    if (hasSocket) {
      _frame += 1;

      // first thing we do is to poll the network
      BoltCore.PollNetwork();

      // step remote rpcs and entities which depends on remote esimate frame numbers
      BoltCore.StepRemoteFrames();

      // step entities which we in some way are controlling locally
      BoltEntity entity = null;
      var entityIter = _entities.GetIterator();

      while (entityIter.Next(out entity)) {
        if (entity.isOwner) {
          entity.SimulateStep();
        }
        else {
          if (entity.hasControl) {
            entity.SimulateStep();
          }
        }
      }
    }
  }

  static void HandleConnected(UdpConnection udp) {
    Assert.True(udp.uid > 1);

    if (isClient) {
      _uid = udp.uid;
      BoltLog.Debug("connected as connection uid {0}", udp.uid);
    }

    BoltConnection cn = new BoltConnection(udp);

    // put on connection list
    _connections.AddLast(cn);

    // generic connected callback
    BoltCallbacksBase.ConnectedInvoke(cn);

    // invoke callback depending on connection type
    if (cn.udpConnection.IsServer) { BoltCallbacksBase.ClientConnectedInvoke(cn); } else { BoltCallbacksBase.ConnectedToServerInvoke(cn); }

    // load map on clients which connect
    if (isServer) {
      if (_mapLoadState.scene.name != null) {
        cn.LoadMapOnClient(_mapLoadState.scene);
      }
      else {
        BoltLog.Warn("{0} connected without server having a map loading or loaded", cn);
      }
    }
  }

  static void HandleDisconnected(BoltConnection cn) {
    // invoke callback depending on connection type
    if (cn.udpConnection.IsServer) { BoltCallbacksBase.ClientDisconnectedInvoke(cn); } else { BoltCallbacksBase.DisconnectedFromServerInvoke(cn); }

    // generic disconnected callback
    BoltCallbacksBase.DisconnectedInvoke(cn);

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

  static void UpdateScope() {
    BoltConnection cn;
    var cnIter = _connections.GetIterator();

    while (cnIter.Next(out cn)) {
      // if this connection isn't allowed to proxy objects, skip it
      if (cn._remoteMapLoadState.stage != SceneLoadStage.CallbackDone) {
        continue;
      }

      BoltEntity en = null;
      var enIter = _entities.GetIterator();

      while (enIter.Next(out en)) {
        // if proxying is disabled for this object, skip it
        if (en._flags & BoltEntity.FLAG_DISABLE_PROXYING) { continue; }

        // if this object originates from this connection, skip it
        if (ReferenceEquals(en._source, en)) { continue; }

        // a controlling connection is always considered in scope
        bool scope = en.boltSerializer.InScope(cn) || ReferenceEquals(en._remoteController, cn);
        bool exists = cn._entityChannel.ExistsOnRemote(en);

        // if we DO exists on remote but ARE NOT in scope
        // anymore, we should mark the proxy for deletion
        if (exists && !scope) {
          cn._entityChannel.DestroyOnRemote(en, BoltEntityDestroyMode.OutOfScope);
        }

        // if we DO NOT exist on remote but ARE in scope
        // we should create a new proxy on this connection
        if (!exists && scope) {
          cn._entityChannel.CreateOnRemote(en);
        }
      }
    }
  }

  static internal void UpdateActiveGlobalBehaviours(string map) {
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

    CreateGlobalBehaviour(typeof(BoltPoll));
    CreateGlobalBehaviour(typeof(BoltSend));
    CreateGlobalBehaviour(typeof(BoltSceneLoader));

    if (isServer) {
      CreateGlobalBehaviour(typeof(BoltEventServerReceiver));
      DeleteGlobalBehaviour(typeof(BoltEventClientReceiver));
    }
    else {
      DeleteGlobalBehaviour(typeof(BoltEventServerReceiver));
      CreateGlobalBehaviour(typeof(BoltEventClientReceiver));
    }

    foreach (var pair in _globalBehaviours) {
      if ((pair.item0.mode & _mode) == _mode) {
        var anyMap = pair.item0.maps.Length == 0;
        var matchesMap = Array.IndexOf(pair.item0.maps, map) != -1;

        if (anyMap || matchesMap) {
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
        BoltLog.Debug("creating '{0}'", t);
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
        BoltLog.Debug("deleting '{0}'", t);
        Component.Destroy(component);
      }
    }
  }

  static void Initialize(BoltNetworkModes mode, UdpEndPoint endpoint, IBoltNetwork autogen, BoltConfig config) {
    var isServer = mode == BoltNetworkModes.Server;
    var isClient = mode == BoltNetworkModes.Client;

    // close any existing socket
    Shutdown();

    if (isServer) {
      _uid = 1;
    }

    // init loggers
    var fileLog = (config.logTargets & BoltConfigLogTargets.File) == BoltConfigLogTargets.File;
    var unityLog = (config.logTargets & BoltConfigLogTargets.Unity) == BoltConfigLogTargets.Unity;
    var consoleLog = (config.logTargets & BoltConfigLogTargets.Console) == BoltConfigLogTargets.Console;
    var systemOutLog = (config.logTargets & BoltConfigLogTargets.SystemOut) == BoltConfigLogTargets.SystemOut;

    if (unityLog) { BoltLog.Add(new BoltLog.Unity()); }
    if (consoleLog) { BoltLog.Add(new BoltLog.Console()); }
    if (systemOutLog) { BoltLog.Add(new BoltLog.SystemOut()); }
    if (fileLog) {
      switch (Application.platform) {
        case RuntimePlatform.OSXEditor:
        case RuntimePlatform.WindowsEditor:
        case RuntimePlatform.WindowsPlayer:
        case RuntimePlatform.OSXPlayer:
          BoltLog.Add(new BoltLog.File());
          break;
      }
    }

    // set config
    _config = config;

    // set frametime
    Time.fixedDeltaTime = 1f / (float)config.framesPerSecond;

    // set udpkits log writer
    UdpLog.SetWriter(UdpLogWriter);

    // :)
    BoltLog.Debug("bolt starting at {0} fps / {1} frametime", config.framesPerSecond, Time.fixedDeltaTime);

    // locate global object types
    _userAssemblyHash = BoltRuntimeReflection.GetUserAssemblyHash();

    // create the gflobal 'Bolt' unity object
    if (_globalBehaviourObject) {
      GameObject.Destroy(_globalBehaviourObject);
    }

    _globalBehaviours = BoltRuntimeReflection.FindGlobalObjectTypes();
    _globalBehaviourObject = new GameObject("Bolt");

    GameObject.DontDestroyOnLoad(_globalBehaviourObject);

    // unregister all handlers
    Assert.True(BoltFactory.IsEmpty);

    // register our handlers
    BoltFactory.Register(new LoadMapFactory());
    BoltFactory.Register(new LoadMapDoneFactory());

    // setup autogen and mode
    _mode = mode;
    _autogen = autogen;
    _autogen.Setup();

    // setup udpkit configuration
    _udpConfig = new UdpConfig();
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
    _udpConfig.PacketSize = Mathf.Clamp(_config.packetSize, 1024, 4096);
    _udpConfig.UseAvailableEventEvent = false;

    //var userHash = GetUserAssemblyHash();
    //if (_config.useAssemblyChecksum && userHash != null && userHash.Length == 16) {
    //  _udpConfig.HandshakeData = new UdpHandshakeData[1];
    //  _udpConfig.HandshakeData[0] = new UdpHandshakeData("ApplicationGUID", new Guid(_config.applicationGuid).ToByteArray());
    //  _udpConfig.HandshakeData[1] = new UdpHandshakeData("AssemblyHash", GetUserAssemblyHash());
    //} else {
    //_udpConfig.HandshakeData = new UdpHandshakeData[1];
    //_udpConfig.HandshakeData[0] = new UdpHandshakeData("ApplicationGUID", new Guid(_config.applicationGuid).ToByteArray());
    //}

    // create and start socket
    _udpSocket = UdpSocket.Create(BoltRuntimeReflection.InvokeCreatePlatformMethod(), () => new BoltSerializer(), _udpConfig);
    _udpSocket.Start(endpoint);

    // init all global behaviours
    UpdateActiveGlobalBehaviours(null);
  }

  static UdpNoise CreatePerlinNoise() {
    var x = UnityEngine.Random.value;
    var s = Stopwatch.StartNew();
    return () => Mathf.PerlinNoise(x, (float)s.Elapsed.TotalSeconds);
  }

  static UdpNoise CreateRandomNoise() {
    var r = new System.Random();
    return () => (float)r.NextDouble();
  }

  static void UdpLogWriter(uint level, string message) {
    switch (level) {
#if DEBUG
      case UdpLog.DEBUG:
      case UdpLog.TRACE:
        BoltLog.Debug(message);
        break;
#endif

      case UdpLog.USER:
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
  }

  internal static void LoadMapBeginInternal(Scene map) {
    {
      var it = _entities.GetIterator();

      while (it.Next()) {
        if (it.val.isOwner && (it.val._persistanceMode == BoltEntityPersistanceMode.DestroyOnLoad)) {
          Destroy(it.val);
        }
      }
    }

    if (isServer) {
      var it = _connections.GetIterator();

      while (it.Next()) {
        it.val.LoadMapOnClient(map);
      }
    }

    // call out to user code
    BoltCallbacksBase.SceneLoadLocalBeginInvoke(map.name);

    // destroy old behaviours
    UpdateActiveGlobalBehaviours(null);
  }

  internal static void LoadMapDoneInternal(Scene map) {
    Assert.True(_mapLoadState.stage == SceneLoadStage.Load);
    _mapLoadState = _mapLoadState.FinishLoad(_mapLoadState.scene, _mapLoadState.scene);
    Assert.True(_mapLoadState.stage == SceneLoadStage.LoadDone);
    _mapLoadState = _mapLoadState.BeginCallback(_mapLoadState);
    Assert.True(_mapLoadState.stage == SceneLoadStage.Callback);
    _mapLoadState = _mapLoadState.FinishCallback(_mapLoadState.scene);
    Assert.True(_mapLoadState.stage == SceneLoadStage.CallbackDone);

    BoltIterator<BoltConnection> it;

    it = _connections.GetIterator();
    while (it.Next()) {
      it.val.SendMapLoadDoneToRemote();
    }

    // update active behaviours
    UpdateActiveGlobalBehaviours(map.name);

    // call out to sure code
    BoltCallbacksBase.SceneLoadLocalDoneInvoke(map.name);

    it = _connections.GetIterator();
    while (it.Next()) {
      it.val.TriggerRemoteMapDoneCallbacks();
    }
  }
}
