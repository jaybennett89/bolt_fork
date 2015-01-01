using Bolt;
using System;

/// <summary>
/// Sets the network mode and scenes that a Bolt.GlobalEventListener should be run on
/// </summary>
/// <example>
/// *Example:* Setting up a server-side event listener to spawn players
/// 
/// ```csharp
/// [BoltGlobalBehaviour(BoltNetworkModes.Server)]
/// public class BoltServerCallbacks: Bolt.GlobalEventListener {
///   public override void SceneLoadLocalDone(string map) {
///     SpawnServerPlayer();
///   }
///   
///   public override void SceneLoadRemoteDone(BoltConnection connection) {
///     SpawnRemotePlayer(connection);
///   }
/// }
/// ```
/// </example>
[DocumentationAttribute]
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class BoltGlobalBehaviourAttribute : Attribute {

  /// <summary>
  /// Sets this behaviour to run only in server or client network mode
  /// </summary>
  public BoltNetworkModes Mode {
    get;
    private set;
  }

  /// <summary>
  /// A list of scenes for this behaviour to run on
  /// </summary>
  public string[] Scenes {
    get;
    private set;
  }

  public BoltGlobalBehaviourAttribute()
    : this(BoltNetworkModes.Server | BoltNetworkModes.Client) {
  }

  public BoltGlobalBehaviourAttribute(BoltNetworkModes mode)
    : this(mode, new string[0]) {
  }

  public BoltGlobalBehaviourAttribute(params string[] scenes)
    : this(BoltNetworkModes.Server | BoltNetworkModes.Client, scenes) {
  }

  public BoltGlobalBehaviourAttribute(BoltNetworkModes mode, params string[] scenes) {
    this.Mode = mode;
    this.Scenes = scenes;
  }
}