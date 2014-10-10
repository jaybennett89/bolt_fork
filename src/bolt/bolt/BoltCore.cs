﻿using Bolt;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UdpKit;
using UnityEngine;
using UE = UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public enum BoltNetworkModes {
  None = 0,
  Server = 1,
  Client = 2,
}

internal static class BoltCore {
  static UdpSocket _udpSocket;
  static internal SceneLoadState _localSceneLoading;

  static internal IPrefabPool PrefabPool = new DefaultPrefabPool();

  static internal int _frame = 0;
  static internal BoltNetworkModes _mode = BoltNetworkModes.None;

  static internal BoltConfig _config = null;
  static internal UdpConfig _udpConfig = null;

  static internal BoltDoubleList<Entity> _entities = new BoltDoubleList<Entity>();
  static internal BoltDoubleList<BoltConnection> _connections = new BoltDoubleList<BoltConnection>();
  static internal Bolt.EventDispatcher _globalEventDispatcher = new Bolt.EventDispatcher();

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

  public static void Destroy(BoltEntity entity) {
    if (!entity.isOwner) {
      BoltLog.Warn("Only the owner can destroy an entity, ignoring call to Destroy().");
      return;
    }

    if (!entity.isAttached) {
      BoltLog.Warn("Entity is not attached, ignoring call to Destroy().");
      return;
    }

    DestroyForce(entity.Entity);
  }

  internal static void DestroyForce(Bolt.Entity entity) {
    // detach
    entity.Detach();

    // destroy
    PrefabPool.Destroy(entity.UnityObject.gameObject);
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

  public static BoltEntity Instantiate(PrefabId prefabId, Vector3 position, Quaternion rotation) {
    return Instantiate(PrefabPool.LoadPrefab(prefabId), position, rotation);
  }

  public static BoltEntity Instantiate(GameObject prefab, Vector3 position, Quaternion rotation) {
    BoltEntity be = prefab.GetComponent<BoltEntity>();
    return Instantiate(new PrefabId(be._prefabId), new TypeId(be._defaultSerializerTypeId), position, rotation);


  }

  public static BoltEntity Instantiate(PrefabId prefabId, TypeId serializerId, UE.Vector3 position, UE.Quaternion rotation) {
    // prefab checks
    {
      GameObject prefab = PrefabPool.LoadPrefab(prefabId);
      BoltEntity be = prefab.GetComponent<BoltEntity>();

      if (isClient && (be._allowInstantiateOnClient == false)) {
        throw new BoltException("This prefab is not allowed to be instantiated on clients");
      }

      if (be._prefabId != prefabId.Value) {
        throw new BoltException("PrefabId for BoltEntity component did not return the same value as prefabId passed in as argument to Instantiate");
      }
    }

    Entity eo;
    eo = Entity.CreateFor(prefabId, serializerId, position, rotation);
    eo.Initialize();
    eo.Attach();

    return eo.UnityObject;
  }

  public static GameObject Attach(GameObject gameObject) {
    BoltEntity be = gameObject.GetComponent<BoltEntity>();
    return Attach(gameObject, new TypeId(be._defaultSerializerTypeId));
  }

  public static GameObject Attach(GameObject gameObject, TypeId serializerId) {
    BoltEntity be = gameObject.GetComponent<BoltEntity>();

    Entity en;
    en = Entity.CreateFor(gameObject, new PrefabId(be._prefabId), serializerId);
    en.Initialize();
    en.Attach();
    
    return en.UnityObject.gameObject;
  }

  public static void Detach(BoltEntity entity) {
    Assert.NotNull(entity.Entity);
    entity.Entity.Detach();
  }

  public static Bolt.Entity FindEntity(InstanceId id) {
    var it = _entities.GetIterator();

    while (it.Next()) {
      if (it.val.InstanceId == id) {
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
      BoltLog.Info("shutting down");

      try {
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
          HandleDisconnected(ev.Connection.GetBoltConnection());
          break;

        case UdpEventType.ConnectRequest:
          BoltInternal.GlobalEventListenerBase.ConnectRequestInvoke(ev.EndPoint, ev.Object0 as byte[]);
          break;

        case UdpEventType.ConnectFailed:
          BoltInternal.GlobalEventListenerBase.ConnectFailedInvoke(ev.EndPoint);
          break;

        case UdpEventType.ConnectRefused:
          BoltInternal.GlobalEventListenerBase.ConnectRefusedInvoke(ev.EndPoint);
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
      BoltLog.Error("AcceptConnection can only be called on the server");
      return;
    }

    if (_config.serverConnectionAcceptMode != BoltConnectionAcceptMode.Manual) {
      BoltLog.Warn("AcceptConnection can only be called BoltConnectionAcceptMode is set to Manual");
      return;
    }

    _udpSocket.Accept(endpoint, userToken);
  }

  public static void RefuseConnection(UdpEndPoint endpoint) {
    if (!isServer) {
      BoltLog.Error("RefuseConnection can only be called on the server");
      return;
    }

    if (_config.serverConnectionAcceptMode != BoltConnectionAcceptMode.Manual) {
      BoltLog.Warn("RefuseConnection can only be called BoltConnectionAcceptMode is set to Manual");
      return;
    }

    _udpSocket.Refuse(endpoint);
  }

  internal static void Send() {
    if (hasSocket) {
      BoltPhysics.SnapshotWorld();

      // switch perf counters
      if ((_frame % framesPerSecond) == 0) {
        var it = _connections.GetIterator();

        while (it.Next()) {
          it.val.SwitchPerfCounters();
        }
      }

      if ((_frame % localSendRate) == 0) {
        // send data on all connections
        var it = _connections.GetIterator();

        while (it.Next()) {
          it.val.Send();
        }
      }
    }
  }

  internal static void FixedUpdate() {
    if (hasSocket) {
      _frame += 1;

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
        if (iter.val.IsOwner || (iter.val.HasControl && iter.val.ControllerLocalPrediction)) {
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
    BoltConnection cn = new BoltConnection(udp);

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

  static void HandleDisconnected(BoltConnection cn) {
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

    foreach (var pair in _globalBehaviours) {
      if ((pair.item0.Mode & _mode) == _mode) {
        var anyMap = pair.item0.Scenes.Length == 0;
        var matchesMap = Array.IndexOf<int>(pair.item0.Scenes, index + BoltNetworkInternal.SceneIndexOffset) != -1;
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

  internal static void Initialize(BoltNetworkModes mode, UdpEndPoint endpoint, BoltConfig config) {
    var isServer = mode == BoltNetworkModes.Server;
    var isClient = mode == BoltNetworkModes.Client;

    // close any existing socket
    Shutdown();

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
    _localSceneLoading = SceneLoadState.DefaultLocal();

    _udpSocket = UdpSocket.Create(BoltNetworkInternal.CreateUdpPlatform(), () => new BoltSerializer(), _udpConfig);
    _udpSocket.Start(endpoint);

    // init all global behaviours
    UpdateActiveGlobalBehaviours(-1);
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

  internal static void SceneLoadBegin(Scene scene) {
    var it = _entities.GetIterator();

    while (it.Next()) {
      if (it.val.IsOwner && (it.val.PersistsOnSceneLoad == false)) {
        DestroyForce(it.val);
      }
    }

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

    // call out to user code
    BoltInternal.GlobalEventListenerBase.SceneLoadLocalDoneInvoke(BoltNetworkInternal.GetSceneName(scene.Index));
  }
}
