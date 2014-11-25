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

public enum BoltNetworkModes {
  None = 0,
  Server = 1,
  Client = 2,
}


internal static class BoltCore {
  static UdpSocket _udpSocket;
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
  static internal List<BoltEntity> _sceneObjects = new List<BoltEntity>();

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

  public static void LoadScene(int index) {
    if (isServer == false) {
      BoltLog.Error("You are not the server, only the server can initiate a scene load");
      return;
    }

    // pass to internal call
    LoadSceneInternal(_localSceneLoading.BeginLoad(index));
  }


  internal static void LoadSceneInternal(SceneLoadState loading) {
    // update
    _localSceneLoading = loading;

    // begin loading
    BoltSceneLoader.Enqueue(_localSceneLoading.Scene);
  }

  public static void Shutdown() {
    if (_udpSocket != null && _mode != BoltNetworkModes.None) {
      // log that we are shutting down
      BoltLog.Info("Shutdown");

      // notify user code
      BoltInternal.GlobalEventListenerBase.BoltShutdownInvoke();

      UPnP.Disable(false);

      // 
      _mode = BoltNetworkModes.None;

      // disconnect from everywhere
      foreach (BoltConnection connection in _connections) {
        connection.Disconnect();
      }

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

      _udpSocket.Close();
      _udpSocket = null;

      Factory.UnregisterAll();
      BoltLog.RemoveAll();
    }
  }

  public static UdpSession[] GetSessions() {
    return _udpSocket.GetSessions();
  }

  public static void Connect(UdpEndPoint endpoint) {
    Connect(endpoint, null);
  }

  public static void Connect(UdpEndPoint endpoint, IProtocolToken token) {
    if (server != null) {
      BoltLog.Error("You must disconnect from the current server first");
      return;
    }

    // stop broadcasting
    DisableLanBroadcast();

    // connect
    _udpSocket.Connect(endpoint, (token == null) ? null : token.ToByteArray());
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
      BoltLog.Error("Incorrect broadcast endpoint: {0}", endpoint);
    }
    else {
      _udpSocket.EnableLanBroadcast(endpoint, isServer);
    }
  }

  public static void DisableLanBroadcast() {
    _udpSocket.DisableLanBroadcast();
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
          if (it.val.StepRemoteFrame()) {
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
          HandleDisconnected(ev.Connection.GetBoltConnection(), (byte[])ev.Object0);
          break;

        case UdpEventType.ConnectRequest:
          HandleConnectRequest(ev.EndPoint, (byte[])ev.Object0);
          break;

        case UdpEventType.ConnectFailed:
          HandleConnectFailed(ev.EndPoint);
          break;

        case UdpEventType.ConnectRefused:
          HandleConnectRefused(ev.EndPoint, (byte[])ev.Object0);
          break;

        case UdpEventType.ConnectAttempt:
          BoltInternal.GlobalEventListenerBase.ConnectAttemptInvoke(ev.EndPoint);
          break;

        case UdpEventType.PacketLost:
          ev.Connection.GetBoltConnection().PacketLost((Packet)ev.Packet.UserToken);
          break;

        case UdpEventType.PacketDelivered:
          ev.Connection.GetBoltConnection().PacketDelivered((Packet)ev.Packet.UserToken);
          break;

        case UdpEventType.PacketReceived:
          ev.Connection.GetBoltConnection().PacketReceived(ev.Packet);
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

  public static void AcceptConnection(UdpEndPoint endpoint, object userToken, IProtocolToken protocolToken) {
    if (!isServer) {
      BoltLog.Error("AcceptConnection can only be called on the server");
      return;
    }

    if (_config.serverConnectionAcceptMode != BoltConnectionAcceptMode.Manual) {
      BoltLog.Warn("AcceptConnection can only be called BoltConnectionAcceptMode is set to Manual");
      return;
    }

    _udpSocket.Accept(endpoint, userToken, protocolToken.ToByteArray());
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
        var localNotLoadingAndCanReceive = (BoltSceneLoader.IsLoading == false) && _canReceiveEntities;

        while (it.Next()) {
          var sendRateMultiplier = it.val.SendRateMultiplier;
          var remoteNotLoadingAndCanReceive = (it.val.isLoadingMap == false) && it.val._canReceiveEntities;
          var modifiedSendRate = localSendRate * sendRateMultiplier;

          // if both connection and local can receive entities, use local sendrate
          if (localNotLoadingAndCanReceive && remoteNotLoadingAndCanReceive && ((_frame % modifiedSendRate) == 0)) {
            it.val.Send();

            if (sendRateMultiplier != 1) {
              BoltLog.Debug("Send Rate: {0} / {1}", modifiedSendRate, sendRateMultiplier);
            }
          }

          // if not, only send 1 packet/second
          else if ((_frame % framesPerSecond) == 0) {
            it.val.Send();
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
      if (UE.Time.timeScale != 1f) {
        BoltLog.Error("Time.timeScale is not 1, value: {0}", UE.Time.timeScale);
      }

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
        if (iter.val.IsOwner || iter.val.HasPredictedControl) {
          iter.val.Simulate();
        }
      }

      Bolt.EventDispatcher.DispatchAllEvents();
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

  internal static void Initialize(BoltNetworkModes mode, UdpEndPoint endpoint, BoltConfig config) {
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

#if DEBUG
    DebugInfo.ignoreList = new HashSet<NetworkId>();

    if (BoltRuntimeSettings.instance.showDebugInfo) {
      DebugInfo.Show();
    }

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
    _udpConfig.PacketSize = Mathf.Clamp(_config.packetSize, 1024, 4096);
    _udpConfig.UseAvailableEventEvent = false;

    // create and start socket
    _localSceneLoading = SceneLoadState.DefaultLocal();

    //_udpSocket = UdpSocket.Create(new UdpKit.UdpPlatformManaged(), () => new BoltSerializer(), _udpConfig);
    _udpSocket = new UdpSocket(BoltNetworkInternal.CreateUdpPlatform(), _udpConfig);
    _udpSocket.Start(endpoint);

    // init all global behaviours
    UpdateActiveGlobalBehaviours(-1);

    // tell user that we started
    BoltInternal.GlobalEventListenerBase.BoltStartedInvoke();
  }

#if DEBUG
  static UdpNoise CreatePerlinNoise() {
    var x = UnityEngine.Random.value;
    var s = Stopwatch.StartNew();
    return () => Mathf.PerlinNoise(x, (float)s.Elapsed.TotalSeconds);
  }

  static UdpNoise CreateRandomNoise() {
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

  internal static void SceneLoadBegin(Scene scene) {
    var it = _entities.GetIterator();

    while (it.Next()) {
      if (it.val.IsOwner && (it.val.PersistsOnSceneLoad == false)) {
        DestroyForce(it.val);
      }
    }

    // clear out scene entities
    _sceneObjects = new List<BoltEntity>();

    // update behaviours
    UpdateActiveGlobalBehaviours(scene.Index);

    // call out to user code
    BoltInternal.GlobalEventListenerBase.SceneLoadLocalBeginInvoke(BoltNetworkInternal.GetSceneName(scene.Index));
  }

  internal static void SceneLoadDone(Scene scene) {
    // verify state
    Assert.True(_localSceneLoading.Scene == scene);
    Assert.True(_localSceneLoading.State == SceneLoadState.STATE_LOADING);

    // switch local state
    _localSceneLoading.State = SceneLoadState.STATE_LOADING_DONE;

    // grab all scene entities
    _sceneObjects =
      UE.GameObject.FindObjectsOfType<BoltEntity>()
        .Where(x => !x.isAttached && x.sceneGuid != UniqueId.None)
        .ToList();

    // update settings
    foreach (var se in _sceneObjects) {
      using (var mod = se.ModifySettings()) {
        mod.clientPredicted = false;
        mod.persistThroughSceneLoads = false;
        mod.allowInstantiateOnClient = false;
      }

      // attach on server
      if (isServer) {
        BoltEntity entity;
        entity = Attach(se.gameObject, EntityFlags.SCENE_OBJECT).GetComponent<BoltEntity>();
        entity.Entity.SceneId = se.sceneGuid;
      }
    }

    BoltLog.Debug("Found {0} Scene Objects", _sceneObjects.Count);

    // call out to user code
    BoltInternal.GlobalEventListenerBase.SceneLoadLocalDoneInvoke(BoltNetworkInternal.GetSceneName(scene.Index));
  }

  internal static GameObject FindSceneObject(UniqueId uniqueId) {
    BoltEntity entity = _sceneObjects.FirstOrDefault(x => x.sceneGuid == uniqueId);

    if (entity) {
      return entity.gameObject;
    }

    return null;
  }

}
