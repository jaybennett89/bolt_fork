//#define COLOR_EDITOR

using Bolt;
using System;
using System.IO;
using System.Linq;
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

    BoltEditorGUI.WithLabel("FixedUpdate Rate", () => {
      settings._config.framesPerSecond = BoltEditorGUI.IntFieldOverlay(settings._config.framesPerSecond, "Per Second");
    });

    BoltEditorGUI.WithLabel("Network Tick Rate", () => {
      settings._config.serverSendRate = BoltEditorGUI.IntFieldOverlay(settings._config.serverSendRate, "Frames");
    });

    BoltEditorGUI.WithLabel("Max Priorities", () => {
      settings._config.maxEntityPriority = Mathf.Clamp(BoltEditorGUI.IntFieldOverlay(settings._config.maxEntityPriority, "Entity"), 1, 1 << 16);
      settings._config.maxPropertyPriority = Mathf.Clamp(BoltEditorGUI.IntFieldOverlay(settings._config.maxPropertyPriority, "Proprerty"), 1, 1 << 11);
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
    });

    BoltEditorGUI.WithLabel("Instantiate Mode", () => {
      settings.clientCanInstantiateAll = BoltEditorGUI.ToggleDropdown("Client Can Instantiate Everything", "Individual On Each Prefab", settings.clientCanInstantiateAll);
    });

    if ((settings._config.scopeMode == Bolt.ScopeMode.Manual) && (settings.scopeModeHideWarningInGui == false)) {
      EditorGUILayout.HelpBox("When manual scoping is enabled you are required to call BoltEntity.SetScope for each connection that should receive a replicated copy of the entity.", MessageType.Warning);

      if (GUILayout.Button("I understand, hide this warning", EditorStyles.miniButton)) {
        settings.scopeModeHideWarningInGui = true;
        Save();
      }
    }

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

    BoltEditorGUI.WithLabel("Accept Token Size", () => {
      settings._config.connectionTokenSize = Mathf.Clamp(BoltEditorGUI.IntFieldOverlay(settings._config.connectionTokenSize, "Bytes"), 0, UdpKit.UdpSocket.MaxTokenSize);
    });

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
    File.Move(a, b);

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
      EditorGUILayout.HelpBox("Bolt is compiled in release mode, these settings have no effectr", MessageType.Warning);
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
      PrefabDatabase.Instance.ManualMode = BoltEditorGUI.ToggleDropdown("Manual", "Automatic", PrefabDatabase.Instance.ManualMode);
    });
  }

  Vector2 scrollPos = Vector2.zero;

  void Header(string text, string icon) {
    //GUILayout.BeginHorizontal(BoltEditorGUI.HeaderBackgorund, GUILayout.Height(BoltEditorGUI.HEADER_HEIGHT));
    BoltEditorGUI.Header(text, icon);
    //GUILayout.EndHorizontal();
  }

  void OnGUI() {
    scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

    Header("Replication", "mc_replication");
    Replication();

    Header("Connection", "mc_connection");
    Connection();

    Header("Latency Simulation", "mc_ping_sim");
    Simulation();

    Header("Miscellaneous", "mc_settings");
    Miscellaneous();

    Header("Compiler", "mc_compile");
    Compiler();


#if COLOR_EDITOR
    var v = BoltEditorSkin.Selected.Variation;

    v.TintColor = EditorGUILayout.ColorField(v.TintColor);
    v.IconColor = EditorGUILayout.ColorField(v.IconColor);

    BoltEditorSkin s;
    
    s = BoltEditorSkin.Selected;
    s.Variation = v;

    BoltEditorSkin.Selected = s;
#endif

    EditorGUILayout.EndScrollView();

    if (GUI.changed) {
      Save();
    }
  }

  void Save() {
    EditorUtility.SetDirty(BoltRuntimeSettings.instance);
    AssetDatabase.SaveAssets();
  }
}
