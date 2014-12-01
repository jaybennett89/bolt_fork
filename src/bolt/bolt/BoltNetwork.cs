using Bolt;
using System;
using System.Collections.Generic;
using BoltInternal;
using UdpKit;
using UnityEngine;

namespace BoltInternal {
  public interface IDebugDrawer {
    void IsEditor(bool isEditor);
    void Indent(int adjust);
    void Label(string text);
    void LabelBold(string text);
    void LabelField(string text, object value);
    void Separator();
    void SelectGameObject(GameObject gameObject);
  }
}

public static class BoltNetworkInternal {
  public static bool UsingUnityPro;

  public static Action EnvironmentSetup;
  public static Action EnvironmentReset;
  public static IDebugDrawer DebugDrawer;
  public static NatCommunicator NatCommunicator;

  public static Func<UdpPlatform> CreateUdpPlatform;
  public static Func<UdpIPv4Address> GetBroadcastAddress;
  public static Func<int, string> GetSceneName;
  public static Func<string, int> GetSceneIndex;
  public static Func<List<STuple<BoltGlobalBehaviourAttribute, Type>>> GetGlobalBehaviourTypes;

  public static void __Initialize(BoltNetworkModes mode, UdpEndPoint endpoint, BoltConfig config) {
    BoltCore.Initialize(mode, endpoint, config);
  }

  public static void __Shutdown() {
    BoltCore.Shutdown();
  }
}

/// <summary>
/// Holds global methods and properties for starting and
/// stopping bolt, instantiating prefabs and other utils
/// </summary>
[Documentation]
public static class BoltNetwork {
  public static void SetCanReceiveEntities(bool canReceiveEntities) {
    BoltCore._canReceiveEntities = canReceiveEntities;
  }

  /// <summary>
  /// The current local simulation frame number
  /// </summary>
  public static int frame {
    get { return BoltCore.frame; }
  }

  public static int maxConnections {
    get {
      if (isRunning) {
        return isClient ? 1 : BoltCore._config.serverConnectionLimit;
      }

      return 0;
    }
  }

  /// <summary>
  /// The current server simulation frame number
  /// </summary>
  public static IEnumerable<BoltEntity> entities {
    get { return BoltCore.entities; }
  }

  /// <summary>
  /// On the server this returns the local frame, on a client this returns
  /// the currently estimated frame of all server objects we have received
  /// </summary>
  public static int serverFrame {
    get { return BoltCore.serverFrame; }
  }

  /// <summary>
  /// The current server simulation time
  /// </summary>
  public static float serverTime {
    get { return BoltCore.serverTime; }
  }

  /// <summary>
  /// The local time, same as Time.time
  /// </summary>
  public static float time {
    get { return BoltCore.time; }
  }

  /// <summary>
  /// The fixed frame delta, same as Time.fixedDeltaTime
  /// </summary>
  public static float frameDeltaTime {
    get { return BoltCore.frameDeltaTime; }
  }

  /// <summary>
  /// The time the last fixed update begain, same as Time.fixedTime
  /// </summary>
  public static float frameBeginTime {
    get { return BoltCore.frameBeginTime; }
  }

  /// <summary>
  /// Normalized value of how much time have passed since the last FixedUpdate
  /// </summary>
  public static float frameAlpha {
    get { return BoltCore.frameAlpha; }
  }

  /// <summary>
  /// All the connections connected to this host
  /// </summary>
  public static IEnumerable<BoltConnection> connections {
    get { return BoltCore.connections; }
  }

  /// <summary>
  /// All clients connected to this host
  /// </summary>
  public static IEnumerable<BoltConnection> clients {
    get { return BoltCore.clients; }
  }

  /// <summary>
  /// The server connection
  /// </summary>
  public static BoltConnection server {
    get { return BoltCore.server; }
  }

  /// <summary>
  /// How many FixedUpdate frames per second bolt is configured to run
  /// </summary>
  public static int framesPerSecond {
    get { return BoltCore.framesPerSecond; }
  }

  /// <summary>
  /// Returns true if this host is a server
  /// </summary>
  public static bool isServer {
    get { return BoltCore.isServer; }
  }

  public static bool isServerOrNotRunning {
    get { return isServer || (isClient == false); }
  }

  public static bool isConnected {
    get { return isServer || (isClient && BoltCore._connections.count > 0); }
  }

  public static UdpChannelName CreateStreamChannel(string name, UdpChannelMode mode, int priority) {
    return BoltCore.CreateStreamChannel(name, mode, priority);
  }

  //public static UdpStreamData CreateStreamData(byte[] data) {
  //  return BoltCore.CreateStreamData(data);
  //}

  /// <summary>
  /// Returns true if this host is a client
  /// </summary>
  public static bool isClient {
    get { return BoltCore.isClient; }
  }


  /// <summary>
  /// If bolt is running
  /// </summary>
  public static bool isRunning {
    get { return isServer || isClient; }
  }

  /// <summary>
  /// Returns true if Bolt was compiled in debug mode
  /// </summary>
  public static bool isDebugMode {
    get { return BoltCore.isDebugMode; }
  }

  /// <summary>
  /// The scoping mode active
  /// </summary>
  public static ScopeMode scopeMode {
    get { return BoltCore._config.scopeMode; }
  }

  /// <summary>
  /// The global object that all global behaviours will be attached to
  /// </summary>
  public static GameObject globalObject {
    get { return BoltCore.globalObject; }
  }

  /// <summary>
  /// Find an entity based on unique id
  /// </summary>
  /// <param name="id">The id to look up</param>
  /// <returns>The entity if one was found, otherwise null</returns>
  public static BoltEntity FindEntity(NetworkId id) {
    if (id.Packed == 0) {
      return null;
    }

    var it = BoltCore._entities.GetIterator();

    while (it.Next()) {
      if (it.val.IsAttached && it.val.UnityObject && it.val.NetworkId.Packed == id.Packed) {
        return it.val.UnityObject;
      }
    }

    BoltLog.Warn("Could not find entity with {0}", id);
    return null;
  }

  public static void RegisterTokenClass<T>() where T : class, IProtocolToken, new() {
    Factory.RegisterTokenClass(typeof(T));
  }

  public static void EnableUPnP() {
    UPnP.Enable();
  }

  public static void DisableUPnP() {
    UPnP.Disable(true);
  }

  public static void OpenPortUPnP(int port) {
    UPnP.OpenPort(port);
  }

  public static void ClosePortUPnP(int port) {
    UPnP.ClosePort(port);
  }

  public static IEnumerable<INatDevice> NatDevicesUPnP {
    get { return UPnP.NatDevices; }
  }

  public static void SetEventFilter(IEventFilter filter) {
    if (filter == null) {
      throw new ArgumentNullException("filter");
    }

    BoltCore.EventFilter = filter;
  }

  public static void SetPrefabPool(IPrefabPool pool) {
    if (pool == null) {
      throw new ArgumentNullException("pool");
    }

    BoltCore.PrefabPool = pool;
  }

  /*
   * Instantiate
   * 
   * */

  public static BoltEntity Instantiate(GameObject prefab) {
    return Instantiate(prefab, null, Vector3.zero, Quaternion.identity);
  }

  public static BoltEntity Instantiate(GameObject prefab, IProtocolToken token) {
    return Instantiate(prefab, token, Vector3.zero, Quaternion.identity);
  }

  public static BoltEntity Instantiate(GameObject prefab, Vector3 position, Quaternion rotation) {
    return Instantiate(prefab, null, position, rotation);
  }

  public static BoltEntity Instantiate(GameObject prefab, IProtocolToken token, Vector3 position, Quaternion rotation) {
    BoltEntity be = prefab.GetComponent<BoltEntity>();

    if (!be) {
      BoltLog.Error("Prefab '{0}' does not have a Bolt Entity component attached", prefab.name);
      return null;
    }

    if (be.serializerGuid == UniqueId.None) {
      BoltLog.Error("Prefab '{0}' does not have a serializer assigned", prefab.name);
      return null;
    }

    return BoltCore.Instantiate(new PrefabId(be._prefabId), Factory.GetFactory(be.serializerGuid).TypeId, position, rotation, InstantiateFlags.ZERO, null, token);
  }

  public static BoltEntity Instantiate(PrefabId prefabId) {
    return Instantiate(prefabId, null, Vector3.zero, Quaternion.identity);
  }

  public static BoltEntity Instantiate(PrefabId prefabId, IProtocolToken token) {
    return Instantiate(prefabId, token, Vector3.zero, Quaternion.identity);
  }

  public static BoltEntity Instantiate(PrefabId prefabId, Vector3 position, Quaternion rotation) {
    return Instantiate(prefabId, null, position, rotation);
  }

  public static BoltEntity Instantiate(PrefabId prefabId, IProtocolToken token, Vector3 position, Quaternion rotation) {
    return Instantiate(BoltCore.PrefabPool.LoadPrefab(prefabId), token, position, rotation);
  }

  /*
   * Destroy
   * 
   * */

  public static void Destroy(GameObject gameObject) {
    Destroy(gameObject, null);
  }

  public static void Destroy(GameObject gameObject, IProtocolToken token) {
    BoltEntity entity = gameObject.GetComponent<BoltEntity>();

    if (entity) {
      BoltCore.Destroy(entity, token);
    }
    else {
      BoltLog.Error("Can only destroy gameobjects with an BoltEntity component through BoltNetwork.Destroy");
    }
  }

  /*
   * Attach
   * 
   * */

  public static GameObject Attach(GameObject gameObject) {
    return Attach(gameObject, null);
  }

  public static GameObject Attach(GameObject gameObject, IProtocolToken token) {
    return BoltCore.Attach(gameObject, EntityFlags.ZERO, token);
  }

  /*
   * Detach
   * 
   * */

  public static void Detach(GameObject gameObject) {
    Detach(gameObject, null);
  }

  public static void Detach(GameObject gameObject, IProtocolToken token) {
    BoltCore.Detach(gameObject.GetComponent<BoltEntity>(), token);
  }

  /// <summary>
  /// Perform a raycast against Bolt hitboxes
  /// </summary>
  /// <param name="ray"><The ray to/param>
  /// <returns>The hitboxes that intersected the ray</returns>
  public static BoltPhysicsHits RaycastAll(Ray ray) {
    return BoltPhysics.Raycast(ray);
  }

  public static BoltPhysicsHits RaycastAll(Ray ray, int frame) {
    return BoltPhysics.Raycast(ray, frame);
  }

  /// <summary>
  /// Perform a sphere overlap against Bolt hiboxes
  /// </summary>
  /// <param name="origin">The origin of the sphere</param>
  /// <param name="radius">The radius of the sphere</param>
  /// <returns>The hitboxe that overlaps the sphere</returns>
  public static BoltPhysicsHits OverlapSphereAll(Vector3 origin, float radius) {
    return BoltPhysics.OverlapSphere(origin, radius);
  }

  public static BoltPhysicsHits OverlapSphereAll(Vector3 origin, float radius, int frame) {
    return BoltPhysics.OverlapSphere(origin, radius, frame);
  }

  /*
   * Accept
   * */

  public static void Accept(UdpEndPoint endpoint) {
    BoltCore.AcceptConnection(endpoint, null, null, null);
  }

  public static void Accept(UdpEndPoint endpoint, IProtocolToken acceptToken) {
    BoltCore.AcceptConnection(endpoint, null, acceptToken, null);
  }

  public static void Accept(UdpEndPoint endpoint, object userToken) {
    BoltCore.AcceptConnection(endpoint, userToken, null, null);
  }

  public static void Accept(UdpEndPoint endpoint, IProtocolToken acceptToken, IProtocolToken connectToken) {
    BoltCore.AcceptConnection(endpoint, null, acceptToken, connectToken);
  }

  public static void Accept(UdpEndPoint endpoint, object userToken, IProtocolToken acceptToken, IProtocolToken connectToken) {
    BoltCore.AcceptConnection(endpoint, userToken, acceptToken, connectToken);
  }

  /*
   * Refuse
   * */

  public static void Refuse(UdpEndPoint ep) {
    BoltCore.RefuseConnection(ep, null);
  }

  public static void Refuse(UdpEndPoint ep, IProtocolToken token) {
    BoltCore.RefuseConnection(ep, token);
  }

  /// <summary>
  /// Manually add a global event listener
  /// </summary>
  /// <param name="mb">The monobehaviour to invoke events on</param>
  public static void AddGlobalEventListener(MonoBehaviour mb) {
    BoltCore._globalEventDispatcher.Add(mb);
  }

  /// <summary>
  /// Manually remove a global event listener
  /// </summary>
  /// <param name="mb">The monobehaviour to be removed</param>
  public static void RemoveGlobalEventListener(MonoBehaviour mb) {
    BoltCore._globalEventDispatcher.Remove(mb);
  }


  /// <summary>
  /// Load a scene based on name, only possible on the Server
  /// </summary>
  /// <param name="scene">The scene to load</param>
  public static void LoadScene(string scene) {
    BoltCore.LoadScene(BoltNetworkInternal.GetSceneIndex(scene));
  }

  /// <summary>
  /// Connect to a server
  /// </summary>
  /// <param name="endpoint">Server end point to connect to</param>
  public static void Connect(UdpEndPoint endpoint) {
    BoltCore.Connect(endpoint);
  }

  public static void Connect(UdpEndPoint endpoint, IProtocolToken token) {
    BoltCore.Connect(endpoint, token);
  }

  /// <summary>
  /// Set session data for LAN Broadcast/Master Server listing
  /// </summary>
  /// <param name="serverName">Name of the server</param>
  /// <param name="userData">User definable data</param>
  public static void SetSessionData(string serverName, string userData) {
    BoltCore.SetSessionData(serverName, userData);
  }

  /// <summary>
  /// Disable LAN broadcasting
  /// </summary>
  public static void DisableLanBroadcast() {
    BoltCore.DisableLanBroadcast();
  }

  /// <summary>
  /// Enable LAN broadcasting
  /// </summary>
  public static void EnableLanBroadcast() {
    EnableLanBroadcast(60000);
  }

  /// <summary>
  /// Sessions currently vailable from the LAN Broadcasting/Master Server listing
  /// </summary>
  /// <returns>Array of sessions available</returns>
  public static UdpSession[] GetSessions() {
    return BoltCore.GetSessions();
  }

  public static void EnableLanBroadcast(UdpEndPoint endpoint) {
    BoltCore.EnableLanBroadcast(endpoint);
  }

  public static void EnableLanBroadcast(ushort port) {
    EnableLanBroadcast(new UdpEndPoint(BoltNetworkInternal.GetBroadcastAddress(), port));
  }
}
