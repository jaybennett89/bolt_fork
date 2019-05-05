using Bolt;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BoltSettingsWindow : EditorWindow {
  float _lastRepaint;

  void OnEnable() {
    name = title = "Bolt Settings";
    _lastRepaint = 0f;
  }

  void Update() {
    if (_lastRepaint + 0.1f < Time.realtimeSinceStartup) {
      _lastRepaint = Time.realtimeSinceStartup;
      Repaint();
    }
  }

  void Replication() {
    BoltRuntimeSettings settings = BoltRuntimeSettings.instance;

    BoltEditorGUI.WithLabel("Simulation Rate", () => {
      settings._config.framesPerSecond = Mathf.Max(10, BoltEditorGUI.IntFieldOverlay(settings._config.framesPerSecond, "FixedUpdate Calls / Second"));
    });

    BoltEditorGUI.WithLabel("Network Rate", () => {
      settings._config.serverSendRate = Mathf.Clamp(settings._config.serverSendRate, 1, settings._config.framesPerSecond);

      var ssr = settings._config.serverSendRate;
      var fps = settings._config.framesPerSecond;

      string legend = "";

      if (fps == ((fps / ssr) * ssr)) {
        legend = (fps / ssr).ToString();
      }
      else {
        legend = System.Math.Round((float)fps / (float)ssr, 2).ToString();
      }

      settings._config.serverSendRate = Mathf.Clamp(BoltEditorGUI.IntFieldOverlay(settings._config.serverSendRate, string.Format("{0} Packets / Second", legend)), 1, fps);
    });

    BoltEditorGUI.WithLabel("Max Priorities", () => {
      settings._config.maxEntityPriority = Mathf.Clamp(BoltEditorGUI.IntFieldOverlay(settings._config.maxEntityPriority, "Entity"), 1, 1 << 16);
      settings._config.maxPropertyPriority = Mathf.Clamp(BoltEditorGUI.IntFieldOverlay(settings._config.maxPropertyPriority, "Property"), 1, 1 << 11);
    });

    BoltEditorGUI.WithLabel("Disable Dejitter Buffer", () => {
      settings._config.disableDejitterBuffer = EditorGUILayout.Toggle(settings._config.disableDejitterBuffer);
    });

    EditorGUI.BeginDisabledGroup(settings._config.disableDejitterBuffer);

    BoltEditorGUI.WithLabel("Dejitter Delay", () => {
      settings._config.serverDejitterDelayMin = Mathf.Max(0, BoltEditorGUI.IntFieldOverlay(settings._config.serverDejitterDelayMin, "Min"));
      settings._config.serverDejitterDelay = Mathf.Max(1, BoltEditorGUI.IntFieldOverlay(settings._config.serverDejitterDelay, "Frames"));
      settings._config.serverDejitterDelayMax = Mathf.Max(settings._config.serverDejitterDelay + 1, BoltEditorGUI.IntFieldOverlay(settings._config.serverDejitterDelayMax, "Max"));
    });

    EditorGUI.EndDisabledGroup();

    BoltEditorGUI.WithLabel("Scoping Mode", () => {
      Bolt.ScopeMode previous = settings._config.scopeMode;
      settings._config.scopeMode = (Bolt.ScopeMode)EditorGUILayout.EnumPopup(settings._config.scopeMode);

      if (previous != settings._config.scopeMode) {
        settings.scopeModeHideWarningInGui = false;
        Save();
      }

      BoltEditorGUI.Help("https://doc.photonengine.com/en/bolt/current/in-depth/freeze-idle-setscope");
    });

    BoltEditorGUI.WithLabel("Instantiate Mode", () => {
      settings.clientCanInstantiateAll = BoltEditorGUI.ToggleDropdown("Client Can Instantiate Everything", "Individual Setting On Each Prefab", settings.clientCanInstantiateAll);
    });

    if ((settings._config.scopeMode == Bolt.ScopeMode.Manual) && (settings.scopeModeHideWarningInGui == false)) {
      EditorGUILayout.HelpBox("When manual scoping is enabled you are required to call BoltEntity.SetScope for each connection that should receive a replicated copy of the entity.", MessageType.Warning);

      if (GUILayout.Button("I understand, hide this warning", EditorStyles.miniButton)) {
        settings.scopeModeHideWarningInGui = true;
        Save();
      }
    }

    BoltEditorGUI.WithLabel("Override Time Scale", () => {
      settings.overrideTimeScale = EditorGUILayout.Toggle(settings.overrideTimeScale);

      if (!settings.overrideTimeScale) {
        EditorGUILayout.HelpBox("Without override time scale enabled Bolt will not detect any changes to the time scale and set it back to 1.0, you need to handle this manually.", MessageType.Warning);
      }
    });

    settings._config.clientSendRate = settings._config.serverSendRate;
    settings._config.clientDejitterDelay = settings._config.serverDejitterDelay;
    settings._config.clientDejitterDelayMin = settings._config.serverDejitterDelayMin;
    settings._config.clientDejitterDelayMax = settings._config.serverDejitterDelayMax;
  }

  void Connection() {
    BoltRuntimeSettings settings = BoltRuntimeSettings.instance;

    BoltEditorGUI.WithLabel("Limit", () => {
      settings._config.serverConnectionLimit = BoltEditorGUI.IntFieldOverlay(settings._config.serverConnectionLimit, "");
    });

    BoltEditorGUI.WithLabel("Timeout", () => {
      settings._config.connectionTimeout = BoltEditorGUI.IntFieldOverlay(settings._config.connectionTimeout, "ms");
    });

    BoltEditorGUI.WithLabel("Connect Timeout", () => {
      settings._config.connectionRequestTimeout = BoltEditorGUI.IntFieldOverlay(settings._config.connectionRequestTimeout, "ms");
    });

    BoltEditorGUI.WithLabel("Connect Attempts", () => {
      settings._config.connectionRequestAttempts = BoltEditorGUI.IntFieldOverlay(settings._config.connectionRequestAttempts, "");
    });

    BoltEditorGUI.WithLabel("Accept Mode", () => {
      settings._config.serverConnectionAcceptMode = (BoltConnectionAcceptMode)EditorGUILayout.EnumPopup(settings._config.serverConnectionAcceptMode);
    });

    EditorGUI.BeginDisabledGroup(settings._config.serverConnectionAcceptMode != BoltConnectionAcceptMode.Manual);

    EditorGUI.EndDisabledGroup();

    BoltEditorGUI.WithLabel("Packet Size", () => {
      settings._config.packetSize = BoltEditorGUI.IntFieldOverlay(settings._config.packetSize, "Bytes");
    });

    BoltEditorGUI.WithLabel("UPnP", () => {
      EditorGUILayout.BeginVertical();
      EditorGUILayout.BeginHorizontal();

      var dllPath = BoltEditorUtilsInternal.MakePath(Application.dataPath, "bolt", "assemblies", "upnp", "Mono.Nat.dll");
      var bytesPath = BoltEditorUtilsInternal.MakePath(Application.dataPath, "bolt", "assemblies", "upnp", "Mono.Nat.bytes");
      var label = new GUIStyle(GUI.skin.label);

      Color c;
      c = BoltEditorGUI.HighlightColor;
      c.a = File.Exists(dllPath) ? 1f : 0.5f;

      label.normal.textColor = c;

      if (File.Exists(dllPath)) {
        GUILayout.Label("ENABLED", label);

        if (GUILayout.Button("Disable", EditorStyles.miniButton)) {
          DisableCompilerConstant(BuildTargetGroup.Standalone, "BOLT_UPNP_SUPPORT");
          SwitchAsset(dllPath, bytesPath);
        }
      }
      else {
        GUILayout.Label("DISABLED", label);

        if (GUILayout.Button("Enable", EditorStyles.miniButton)) {
          EnableCompilerConstant(BuildTargetGroup.Standalone, "BOLT_UPNP_SUPPORT");
          SwitchAsset(bytesPath, dllPath);
        }
      }

      EditorGUILayout.EndHorizontal();
      EditorGUILayout.EndVertical();
    });
  }

  void SwitchAsset(string a, string b) {
    File.Copy(a, b, true);
	File.Delete(a);
    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    AssetDatabase.ImportAsset(BoltEditorUtilsInternal.MakeAssetPath(b));
    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
  }

  void EnableCompilerConstant(BuildTargetGroup group, string constantToEnable) {
    string constants = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Trim();

    if (!constants.Contains(constantToEnable)) {
      if (string.IsNullOrEmpty(constants)) {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(group, constantToEnable);
      }
      else {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(group, constants + ";" + constantToEnable);
      }
    }
  }

  void DisableCompilerConstant(BuildTargetGroup group, string constantToDisable) {
    string constants = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Trim();

    if (constants.Contains(constantToDisable)) {
      PlayerSettings.SetScriptingDefineSymbolsForGroup(group, constants.Replace(constantToDisable, "").Trim().Trim(';'));
    }
  }

  void Simulation() {
    BoltRuntimeSettings settings = BoltRuntimeSettings.instance;
    EditorGUILayout.BeginVertical();

    if (BoltCore.isDebugMode == false) {
      EditorGUILayout.HelpBox("Bolt is compiled in release mode, these settings have no effect", MessageType.Warning);
    }

    EditorGUI.BeginDisabledGroup(BoltCore.isDebugMode == false);

    BoltEditorGUI.WithLabel("Enabled", () => {
      settings._config.useNetworkSimulation = EditorGUILayout.Toggle(settings._config.useNetworkSimulation);
    });

    EditorGUI.EndDisabledGroup();
    EditorGUI.BeginDisabledGroup(settings._config.useNetworkSimulation == false || BoltCore.isDebugMode == false);

    BoltEditorGUI.WithLabel("Packet Loss", () => {
      int loss;

      loss = Mathf.Clamp(Mathf.RoundToInt(settings._config.simulatedLoss * 100), 0, 100);
      loss = BoltEditorGUI.IntFieldOverlay(loss, "Percent");

      settings._config.simulatedLoss = Mathf.Clamp01(loss / 100f);
    });

    BoltEditorGUI.WithLabel("Ping", () => {
      settings._config.simulatedPingMean = BoltEditorGUI.IntFieldOverlay(settings._config.simulatedPingMean, "Mean");
      settings._config.simulatedPingJitter = BoltEditorGUI.IntFieldOverlay(settings._config.simulatedPingJitter, "Jitter");
    });

    BoltEditorGUI.WithLabel("Noise Function", () => {
      settings._config.simulatedRandomFunction = (BoltRandomFunction)EditorGUILayout.EnumPopup(settings._config.simulatedRandomFunction);
    });

    EditorGUI.EndDisabledGroup();
    EditorGUILayout.EndVertical();
  }

  void Miscellaneous() {
    BoltRuntimeSettings settings = BoltRuntimeSettings.instance;

    BoltEditorGUI.WithLabel("Show Help Buttons", () => {
      settings.showHelpButtons = EditorGUILayout.Toggle(settings.showHelpButtons);
    });

    BoltEditorGUI.WithLabel("Show Entity Settings Hints", () => {
      settings.showBoltEntityHints = EditorGUILayout.Toggle(settings.showBoltEntityHints);
    });

    BoltEditorGUI.WithLabel("Log Targets", () => {
      settings._config.logTargets = (BoltConfigLogTargets)EditorGUILayout.EnumMaskField(settings._config.logTargets);
    });

    BoltEditorGUI.WithLabel("Show Debug Info", () => {
      settings.showDebugInfo = EditorGUILayout.Toggle(settings.showDebugInfo);
    });

    BoltEditorGUI.WithLabel("Log Unity To Console", () => {
      settings.logUncaughtExceptions = EditorGUILayout.Toggle(settings.logUncaughtExceptions);
    });

    var consoleEnabled = (settings._config.logTargets & BoltConfigLogTargets.Console) == BoltConfigLogTargets.Console;
    EditorGUI.BeginDisabledGroup(consoleEnabled == false);

    EditorGUILayout.BeginVertical();

    BoltEditorGUI.WithLabel("Toggle Key", () => {
      settings.consoleToggleKey = (KeyCode)EditorGUILayout.EnumPopup(settings.consoleToggleKey);
    });

    BoltEditorGUI.WithLabel("Visible By Default", () => {
      settings.consoleVisibleByDefault = EditorGUILayout.Toggle(settings.consoleVisibleByDefault);
    });

    EditorGUI.EndDisabledGroup();
    EditorGUILayout.EndVertical();
  }

  void Compiler() {
    BoltRuntimeSettings settings = BoltRuntimeSettings.instance;

    BoltEditorGUI.WithLabel("Warning Level", () => {
      settings.compilationWarnLevel = EditorGUILayout.IntField(settings.compilationWarnLevel);
      settings.compilationWarnLevel = Mathf.Clamp(settings.compilationWarnLevel, 0, 4);
    });

    BoltEditorGUI.WithLabel("Prefab Mode", () => {
      PrefabDatabase.Instance.DatabaseMode = (PrefabDatabaseMode)EditorGUILayout.EnumPopup(PrefabDatabase.Instance.DatabaseMode);
    });
  }

  void MasterServer() {
    BoltRuntimeSettings settings = BoltRuntimeSettings.instance;

    BoltEditorGUI.WithLabel("Game Id", () => {
      if (settings.masterServerGameId == null || settings.masterServerGameId.Trim().Length == 0) {
        settings.masterServerGameId = Guid.NewGuid().ToString().ToUpperInvariant();
        Save();
      }

      GUILayout.BeginVertical();

      settings.masterServerGameId = EditorGUILayout.TextField(settings.masterServerGameId);

      try {
        if (new Guid(settings.masterServerGameId) == Guid.Empty) {
          EditorGUILayout.HelpBox("The game id must non-zero", MessageType.Error);
        }
      }
      catch {
        EditorGUILayout.HelpBox("The game id must be a valid GUID in this format: 00000000-0000-0000-0000-000000000000", MessageType.Error);
      }

      GUILayout.EndVertical();
    });

    BoltEditorGUI.WithLabel("Endpoint", () => {
      settings.masterServerEndPoint = EditorGUILayout.TextField(settings.masterServerEndPoint);
    });

    BoltEditorGUI.WithLabel("Connect", () => {
      settings.masterServerAutoConnect = BoltEditorGUI.ToggleDropdown("Automatic", "Manual", settings.masterServerAutoConnect);
    });

    BoltEditorGUI.WithLabel("Disconnect", () => {
      settings.masterServerAutoDisconnect = BoltEditorGUI.ToggleDropdown("Automatic", "Manual", settings.masterServerAutoDisconnect);
    });
  }

  Vector2 scrollPos = Vector2.zero;

  void Header(string text, string icon) {
    GUIStyle style = new GUIStyle(BoltEditorGUI.HeaderBackgorund);
    style.padding = new RectOffset(5, 0, 4, 4);

    EditorGUILayout.BeginHorizontal(style);

    BoltEditorGUI.Icon(icon);

    GUIStyle s = new GUIStyle(EditorStyles.boldLabel);
    s.margin.top = 0;
    GUILayout.Label(text, s);

    EditorGUILayout.EndHorizontal();
  }

  void OnGUI() {
    scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

    Header("Replication", "mc_replication");
    Replication();

    Header("Connection", "mc_connection");
    Connection();

    Header("Zeus (Master Server)", "mc_masterserver");
    MasterServer();

    Header("Latency Simulation", "mc_ping_sim");
    Simulation();

    Header("Miscellaneous", "mc_settings");
    Miscellaneous();

    Header("Compiler", "mc_compile");
    Compiler();

    EditorGUILayout.EndScrollView();

    if (GUI.changed) {
      Save();
    }
  }

  void Save() {
    EditorUtility.SetDirty(BoltRuntimeSettings.instance);
    EditorUtility.SetDirty(PrefabDatabase.Instance);
    AssetDatabase.SaveAssets();
  }
}
