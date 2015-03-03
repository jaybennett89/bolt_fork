using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Bolt.Documentation(Ignore = true)]
public enum BoltEditorStartMode {
  None = 0,
  Server = 1,
  Client = 2
}

/// <summary>
/// The runtime settings and confugration for the current bolt simulation
/// </summary>
/// <example>
/// *Example:* Using the settings instance to get a copy of the server config.
/// 
/// BoltConfig GetServerConfig() {
///   return BoltRuntimeSettings.instance.GetConfigCopy();
/// }
/// </example>
public class BoltRuntimeSettings : ScriptableObject {
  static BoltRuntimeSettings _instance; 

  /// <summary>
  /// A singleton static instance of the runtime settings
  /// </summary>
  /// <example>
  /// *Example:* Using the runtime settings to create a button mapping to show the bolt console
  /// 
  /// void ConfigureDefault(GameButton button) {
  ///   if(button = GameButton.ShowConsole) { 
  ///     buttonMap.Add(button, BoltRuntimeSettings.instance.consoleToggleKey);
  ///   }
  /// }
  /// </example>
  public static BoltRuntimeSettings instance {
    get {
      if (_instance == null) { 
        _instance = (BoltRuntimeSettings)Resources.Load(typeof(BoltRuntimeSettings).Name, typeof(BoltRuntimeSettings));

        if (_instance == null) {
          BoltLog.Error("Could not find resource: '{0}' ", typeof(BoltRuntimeSettings));
        }
      }

      return _instance;
    }
  }

  [SerializeField]
  internal BoltConfig _config = new BoltConfig();

  /// <summary>
  /// The number of clients to start when launching a debug mode server (Pro-Only)
  /// </summary>
  [SerializeField]
  public int debugClientCount = 1;

  /// <summary>
  /// The default debug start port
  /// </summary>
  [SerializeField]
  public int debugStartPort = 54321;

  /// <summary>
  /// The scene to load after initializing bolt
  /// </summary>
  [SerializeField]
  public string debugStartMapName = null;

  /// <summary>
  /// Whether to play as a server or not
  /// </summary>
  [SerializeField]
  public bool debugPlayAsServer = false;

  /// <summary>
  /// Whether to show debug info or not
  /// </summary>
  [SerializeField]
  public bool showDebugInfo = true;

  /// <summary>
  /// Whether to show debug info or not
  /// </summary>
  [SerializeField]
  public bool overrideTimeScale = true;

  /// <summary>
  /// Whether to log uncaught exceptions
  /// </summary>
  [SerializeField]
  public bool logUncaughtExceptions = false;

  /// <summary>
  /// Whehther the client has instantiate priviledges or not
  /// </summary>
  [SerializeField]
  public bool clientCanInstantiateAll = true;

  [SerializeField]
  public BoltEditorStartMode debugEditorMode = BoltEditorStartMode.Server;

  /// <summary>
  /// The keycode that will toggle visibility of the bolt console
  /// </summary>
  [SerializeField]
  public KeyCode consoleToggleKey = KeyCode.Tab;

  /// <summary>
  /// Whether the bolt console is initially visible or not
  /// </summary>
  [SerializeField]
  public bool consoleVisibleByDefault = true;

  [SerializeField]
  public int compilationWarnLevel = 4;

  [SerializeField]
  public int editorSkin = 4;

  [SerializeField]
  public bool scopeModeHideWarningInGui = false;

  [SerializeField]
  public bool showHelpButtons = true;

  [SerializeField]
  public string masterServerGameId = "";

  [SerializeField]
  public string masterServerEndPoint = "79.99.6.136:24000";

  [SerializeField]
  public bool masterServerAutoConnect = false;

  [SerializeField]
  public bool masterServerAutoDisconnect = true;

  /// <summary>
  /// Get a memberwise copy of the current bolt config
  /// </summary>
  /// <returns>A bolt config</returns>
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
  public BoltConfig GetConfigCopy() {
    return _config.Clone();
  }
}
