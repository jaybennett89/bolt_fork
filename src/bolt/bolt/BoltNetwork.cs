using System;
using System.Collections.Generic;
using UdpKit;
using UnityEngine;

public static class BoltNetworkInternal {
  public static bool UsingUnityPro;

  public static Action EnvironmentSetup;
  public static Action EnvironmentReset;

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
/// Holds global methods and properties for starting and s
/// topping bolt, instantiating prefabs and other utils
/// </summary>
public static class BoltNetwork {
  public static Action ShutdownComplete;

  static BoltNetwork() {
    BoltCore.ShutdownComplete = new Action(ShutdownDone);
  }

  static void ShutdownDone() {
    if (ShutdownComplete != null) {
      ShutdownComplete();
    }

    ShutdownComplete = null;
  }

  /// <summary>
  /// The current local simulation frame number
  /// </summary>
  public static int frame {
    get { return BoltCore.frame; }
  }

  /// <summary>
  /// The current server simulation frame number
  /// </summary>
  public static IEnumerable<BoltEntity> entities {
    get { return BoltCore.entities; }
  }

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

  public static int framesPerSecond {
    get { return BoltCore.framesPerSecond; }
  }

  /// <summary>
  /// Returns true if this host is a server
  /// </summary>
  public static bool isServer {
    get { return BoltCore.isServer; }
  }

  /// <summary>
  /// Returns true if this host is a client
  /// </summary>
  public static bool isClient {
    get { return BoltCore.isClient; }
  }

  /// <summary>
  /// Returns true if Bolt was compiled in debug mode
  /// </summary>
  public static bool isDebugMode {
    get { return BoltCore.isDebugMode; }
  }

  public static GameObject globalObject {
    get { return BoltCore.globalObject; }
  }

  public static void SetInstantiateDestroyCallbacks(Func<GameObject, Vector3, Quaternion, GameObject> instantiate, Action<GameObject> destroy) {
    if (instantiate == null)
      throw new System.ArgumentNullException("instantiate");

    if (destroy == null)
      throw new System.ArgumentNullException("destroy");

    BoltCore._instantiate = instantiate;
    BoltCore._destroy = destroy;
  }

  /// <summary>
  /// Instantiate a prefab and attach it to bolt
  /// </summary>
  /// <param name="prefab">Prefab to instantiate</param>
  /// <returns>The entity instance of the instantiated prefab</returns>
  public static BoltEntity Instantiate(GameObject prefab) {
    return BoltCore.Instantiate(prefab);
  }

  public static BoltEntity Instantiate(GameObject prefab, Vector3 position, Quaternion rotation) {
    return BoltCore.Instantiate(prefab, position, rotation);
  }

  public static BoltEntity Instantiate(string prefab, Vector3 position, Quaternion rotation) {
    return Instantiate(BoltCore.FindPrefab(prefab), position, rotation);
  }

  public static BoltPhysicsHits RaycastAll(Ray ray) {
    return BoltPhysics.Raycast(ray);
  }

  public static BoltPhysicsHits RaycastAll(Ray ray, int frame) {
    return BoltPhysics.Raycast(ray, frame);
  }

  public static BoltPhysicsHits OverlapSphereAll(Vector3 origin, float radius) {
    return BoltPhysics.OverlapSphere(origin, radius);
  }

  public static BoltPhysicsHits OverlapSphereAll(Vector3 origin, float radius, int frame) {
    return BoltPhysics.OverlapSphere(origin, radius, frame);
  }

  public static BoltEntity Attach(BoltEntity entity) {
    return BoltCore.Attach(entity);
  }

  public static void Detach(BoltEntity entity) {
    BoltCore.Detach(entity);
  }

  public static void Accept(UdpEndPoint ep) {
    Accept(ep, null);
  }

  public static void Accept(UdpEndPoint ep, object userToken) {
    BoltCore.AcceptConnection(ep, userToken);
  }

  public static void Refuse(UdpEndPoint ep) {
    BoltCore.RefuseConnection(ep);
  }

  public static BoltEntity Instantiate(string prefab) {
    return Instantiate(BoltCore.FindPrefab(prefab));
  }

  public static void Destroy(BoltEntity entity) {
    BoltCore.Destroy(entity);
  }

  public static void Destroy(GameObject gameobject) {
    BoltCore.Destroy(gameobject);
  }

  public static void LoadScene(int scene) {
    BoltCore.LoadScene(scene);
  }

  public static void LoadScene(string scene) {
    LoadScene(BoltNetworkInternal.GetSceneIndex(scene));
  }

  /// <summary>
  /// Connect to a server
  /// </summary>
  /// <param name="endpoint">Server end point to connect to</param>
  public static void Connect(UdpEndPoint endpoint) {
    BoltCore.Connect(endpoint);
  }

  public static void Connect(UdpEndPoint endpoint, byte[] token) {
    BoltCore.Connect(endpoint, token);
  }

  public static void SetSessionData(string serverName, string userData) {
    BoltCore.SetSessionData(serverName, userData);
  }

  public static void DisableLanBroadcast() {
    BoltCore.DisableLanBroadcast();
  }

  public static void EnableLanBroadcast() {
    EnableLanBroadcast(60000);
  }

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
