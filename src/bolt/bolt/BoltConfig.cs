using System;
using UnityEngine;

namespace Bolt {
  public enum ScopeMode {
    Automatic = 0,
    Manual = 1
  }
}

/// <summary>
/// The type of random function to use for network latency simulation
/// </summary>
/// <example>
/// *Example:*
/// 
/// ```csharp
/// void WriteSettings() {
///   BoltConfig config = BoltRuntimeSettings.instance.GetConfigCopy();
///   
///   if(config.simulatedRandomFunction == BoltRandomFunction.PerlinNoise) {
///     Debug.Log("Using Perlin Noise!");
///   }
///   else {
///     Debug.Log("Using System.Random!");
///   }
/// }
/// ```
/// </example>
public enum BoltRandomFunction {
  PerlinNoise,
  SystemRandom
}

/// <summary>
/// Whether to accept connnections automatically or use the manual process
/// </summary>
/// <example>
/// *Example:*
/// 
/// ```csharp
/// void WriteSettings() {
///   BoltConfig config = BoltRuntimeSettings.instance.GetConfigCopy();
///   
///   if(config.serverConnectionAcceptMode == BoltConnectionAcceptMode.Auto) {
///     Debug.Log("Using Automatic Connection Acceptance");
///   }
///   else {
///     Debug.Log("Using Manual Connection Acceptance");
///   }
/// }
/// ```
/// </example>
public enum BoltConnectionAcceptMode {
  Auto,
  Manual,
}

/// <summary>
/// The target output of bolt logging
/// </summary>
/// <example>
/// *Example:* Conditionally writing to the Unity console depending on the log target of the current config.
/// 
/// ```csharp
/// void WriteExtra(string message) {
///   BoltConfig config = BoltRuntimeSettings.instance.GetConfigCopy();
///   
///   if(config.logTargets == BoltConfigLogTargets.Unity) {
///     Debug.Log(message);
///   }
/// }
/// ```
/// </example>
public enum BoltConfigLogTargets {
  Unity = 1,
  Console = 2,
  File = 4,
  SystemOut = 8
}

/// <summary>
/// Bolt configuration settings object
/// </summary>
/// <example>
/// *Example:* Starting a bolt server simulation internally requires a config parameter (there is a bit more to it not shown here).
/// 
/// ```csharp
/// static void Initialize(BoltNetworkModes modes, UdpEndPoint endpoint, BoltConfig config) {
/// 
///   BoltNetworkInternal.__Initialize(modes, endpoint, config, CreateUdpPlatform());
/// 
/// ```
/// </example>
[Serializable]
public sealed class BoltConfig {

  /// <summary>
  /// The number of fixed updates to the simulation per second
  /// </summary>
  public int framesPerSecond = 60;

  /// <summary>
  /// The UDP packet size in bytes
  /// </summary>
  public int packetSize = 1280;

  /// <summary>
  /// The max data payload size of a packet
  /// </summary>
  public int packetMaxEventSize = 512;

  /// <summary>
  /// The max priority value for updating an entity
  /// </summary>
  public int maxEntityPriority = 1 << 16;

  /// <summary>
  /// The max priority value for updating a property
  /// </summary>
  public int maxPropertyPriority = 1 << 11;

  /// <summary>
  /// Whether to scope connections manually or automatically
  /// </summary>
  public Bolt.ScopeMode scopeMode = Bolt.ScopeMode.Automatic;

  /// <summary>
  /// The output targets to write log messages
  /// </summary>
  public BoltConfigLogTargets logTargets = BoltConfigLogTargets.Console | BoltConfigLogTargets.Unity;

  /// <summary>
  /// Whether dejitter delay buffering is disabled or not
  /// </summary>
  public bool disableDejitterBuffer;

  public int clientSendRate;  
  public int clientDejitterDelay;
  public int clientDejitterDelayMin;
  public int clientDejitterDelayMax;

  public int serverSendRate;
  public int serverDejitterDelay;
  public int serverDejitterDelayMin;
  public int serverDejitterDelayMax;

  /// <summary>
  /// The max number of server connections
  /// </summary>
  public int serverConnectionLimit;

  /// <summary>
  /// Whether to use automatic or manual mode for accepting incoming client connection requests
  /// </summary>
  public BoltConnectionAcceptMode serverConnectionAcceptMode = BoltConnectionAcceptMode.Auto;

  public int commandDejitterDelay;

  /// <summary>
  /// The max number of input commands that can be queued at once
  /// </summary>
  public int commandQueueSize;

  public int commandDelayAllowed;

  /// <summary>
  /// The number of times to redundantly send input commands to the server
  /// </summary>
  public int commandRedundancy;
  public float commandPingMultiplier;

  /// <summary>
  /// Whether to use network latency simulation
  /// </summary>
  public bool useNetworkSimulation = true;

  /// <summary>
  /// The packet loss rate to use in latency simulation
  /// </summary>
  public float simulatedLoss;

  /// <summary>
  /// The mean ping in milliseconds to use in latency simulation
  /// </summary>
  public int simulatedPingMean;

  /// <summary>
  /// The deviation to use in ping simulation
  /// </summary>
  public int simulatedPingJitter;

  /// <summary>
  /// Whether to use Perlin Noise or System.Random function to create ping deviations
  /// </summary>
  public BoltRandomFunction simulatedRandomFunction = BoltRandomFunction.PerlinNoise;

  public int connectionTimeout = 10000;
  public int connectionRequestTimeout = 500;

  /// <summary>
  /// The max number of allowed connection attempts by a single client
  /// </summary>
  public int connectionRequestAttempts = 20;

  public int connectionTokenSize = 128;

  public BoltConfig() {
    // sendrates of server/client
    serverSendRate = 3;
    clientSendRate = 3;

    // interpolation delay on client is based on server rate
    clientDejitterDelay = serverSendRate * 2;
    clientDejitterDelayMin = clientDejitterDelay - serverSendRate;
    clientDejitterDelayMax = clientDejitterDelay + serverSendRate;

    // interpolation delay on server is based on client rate
    serverDejitterDelay = clientSendRate * 2;
    serverDejitterDelayMin = serverDejitterDelay - clientSendRate;
    serverDejitterDelayMax = serverDejitterDelay + clientSendRate;

    // max clients connected to the server
    serverConnectionLimit = 64;

    // commands config
    commandRedundancy = 6;
    commandPingMultiplier = 1.25f;
    commandDejitterDelay = 3;
    commandDelayAllowed = clientDejitterDelay * 2;
    commandQueueSize = 60;
  }

  internal BoltConfig Clone() {
    return (BoltConfig)MemberwiseClone();
  }
}
