using System;
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

    BoltAssetEditorGUI.Label("FixedUpdate Rate", () => {
      settings._config.framesPerSecond = BoltAssetEditorGUI.IntFieldOverlay(settings._config.framesPerSecond, "Per Second");
    });

    BoltAssetEditorGUI.Label("Packet Interval", () => {
      settings._config.serverSendRate = BoltAssetEditorGUI.IntFieldOverlay(settings._config.serverSendRate, "Frames");
    });

    BoltAssetEditorGUI.Label("Dejitter Delay", () => {
      settings._config.serverDejitterDelayMin = BoltAssetEditorGUI.IntFieldOverlay(settings._config.serverDejitterDelayMin, "Min");
      settings._config.serverDejitterDelay = BoltAssetEditorGUI.IntFieldOverlay(settings._config.serverDejitterDelay, "Frames");
      settings._config.serverDejitterDelayMax = BoltAssetEditorGUI.IntFieldOverlay(settings._config.serverDejitterDelayMax, "Max");
    });

    BoltAssetEditorGUI.Label("Scoping Mode", () => {
      Bolt.ScopeMode previous = settings._config.scopeMode;
      settings._config.scopeMode = (Bolt.ScopeMode)EditorGUILayout.EnumPopup(settings._config.scopeMode);

      if (previous != settings._config.scopeMode) {
        settings._config.scopeModeHideWarningInGui = false;
        Save();
      }
    });

    if ((settings._config.scopeMode == Bolt.ScopeMode.Manual) && (settings._config.scopeModeHideWarningInGui == false)) {
      EditorGUILayout.HelpBox("When manual scoping is enabled you are required to call BoltEntity.SetScope for each connection that should receive a replicated copy of the entity.", MessageType.Warning);

      if (GUILayout.Button("I understand, hide this warning", EditorStyles.miniButton)) {
        settings._config.scopeModeHideWarningInGui = true;
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

    BoltAssetEditorGUI.Label("Limit", () => {
      settings._config.serverConnectionLimit = BoltAssetEditorGUI.IntFieldOverlay(settings._config.serverConnectionLimit, "");
    });

    BoltAssetEditorGUI.Label("Timeout", () => {
      settings._config.connectionTimeout = BoltAssetEditorGUI.IntFieldOverlay(settings._config.connectionTimeout, "ms");
    });

    BoltAssetEditorGUI.Label("Connect Timeout", () => {
      settings._config.connectionRequestTimeout = BoltAssetEditorGUI.IntFieldOverlay(settings._config.connectionRequestTimeout, "ms");
    });

    BoltAssetEditorGUI.Label("Connect Attempts", () => {
      settings._config.connectionRequestAttempts = BoltAssetEditorGUI.IntFieldOverlay(settings._config.connectionRequestAttempts, "");
    });

    BoltAssetEditorGUI.Label("Accept Mode", () => {
      settings._config.serverConnectionAcceptMode = (BoltConnectionAcceptMode)EditorGUILayout.EnumPopup(settings._config.serverConnectionAcceptMode);
    });

    EditorGUI.BeginDisabledGroup(settings._config.serverConnectionAcceptMode != BoltConnectionAcceptMode.Manual);

    BoltAssetEditorGUI.Label("Accept Token Size", () => {
      settings._config.connectionTokenSize = Mathf.Clamp(BoltAssetEditorGUI.IntFieldOverlay(settings._config.connectionTokenSize, "Bytes"), 0, UdpKit.UdpSocket.MaxConnectionTokenSize);
    });

    EditorGUI.EndDisabledGroup();

    BoltAssetEditorGUI.Label("Packet Size", () => {

      settings._config.packetSize = BoltAssetEditorGUI.IntFieldOverlay(settings._config.packetSize, "Bytes");
    });
  }

  void Simulation() {
    BoltRuntimeSettings settings = BoltRuntimeSettings.instance;
    EditorGUILayout.BeginVertical();

    if (BoltCore.isDebugMode == false) {
      EditorGUILayout.HelpBox("Bolt is compiled in release mode, these settings have no effectr", MessageType.Warning);
    }

    EditorGUI.BeginDisabledGroup(BoltCore.isDebugMode == false);

    BoltAssetEditorGUI.Label("Enabled", () => {
      settings._config.useNetworkSimulation = EditorGUILayout.Toggle(settings._config.useNetworkSimulation);
    });

    EditorGUI.EndDisabledGroup();
    EditorGUI.BeginDisabledGroup(settings._config.useNetworkSimulation == false || BoltCore.isDebugMode == false);

    BoltAssetEditorGUI.Label("Packet Loss", () => {
      int loss;

      loss = Mathf.Clamp(Mathf.RoundToInt(settings._config.simulatedLoss * 100), 0, 100);
      loss = BoltAssetEditorGUI.IntFieldOverlay(loss, "Percent");

      settings._config.simulatedLoss = Mathf.Clamp01(loss / 100f);
    });

    BoltAssetEditorGUI.Label("Ping", () => {
      settings._config.simulatedPingMean = BoltAssetEditorGUI.IntFieldOverlay(settings._config.simulatedPingMean, "Mean");
      settings._config.simulatedPingJitter = BoltAssetEditorGUI.IntFieldOverlay(settings._config.simulatedPingJitter, "Jitter");
    });

    BoltAssetEditorGUI.Label("Noise Function", () => {
      settings._config.simulatedRandomFunction = (BoltRandomFunction)EditorGUILayout.EnumPopup(settings._config.simulatedRandomFunction);
    });

    EditorGUI.EndDisabledGroup();
    EditorGUILayout.EndVertical();
  }

  void Miscellaneous() {
    BoltRuntimeSettings settings = BoltRuntimeSettings.instance;

    BoltAssetEditorGUI.Label("Compiler Warn Level", () => {
      settings.compilationWarnLevel = EditorGUILayout.IntField(settings.compilationWarnLevel);
      settings.compilationWarnLevel = Mathf.Clamp(settings.compilationWarnLevel, 0, 4);
    });

    BoltAssetEditorGUI.Label("Use Unique Ids", () => {
      settings._config.globalUniqueIds = EditorGUILayout.Toggle(settings._config.globalUniqueIds);
    });

    BoltAssetEditorGUI.Label("Log Targets", () => {
      settings._config.logTargets = (BoltConfigLogTargets)EditorGUILayout.EnumMaskField(settings._config.logTargets);
    });

    if (settings._config.applicationGuid == null || settings._config.applicationGuid.Length != 16) {
      settings._config.applicationGuid = Guid.NewGuid().ToByteArray();
      Save();
    }

    BoltAssetEditorGUI.Label("Editor Highlight Color", () => {
      settings.highlightColor = EditorGUILayout.ColorField(settings.highlightColor);
    });
  }

  void Console() {

    BoltRuntimeSettings settings = BoltRuntimeSettings.instance;

    var consoleEnabled = (settings._config.logTargets & BoltConfigLogTargets.Console) == BoltConfigLogTargets.Console;
    EditorGUI.BeginDisabledGroup(consoleEnabled == false);

    EditorGUILayout.BeginVertical();

    BoltAssetEditorGUI.Label("Toggle Key", () => {
      settings.consoleToggleKey = (KeyCode)EditorGUILayout.EnumPopup(settings.consoleToggleKey);
    });

    BoltAssetEditorGUI.Label("Visible By Default", () => {
      settings.consoleVisibleByDefault = EditorGUILayout.Toggle(settings.consoleVisibleByDefault);
    });

    EditorGUI.EndDisabledGroup();
    EditorGUILayout.EndVertical();
  }

  Vector2 scrollPos = Vector2.zero;

  void OnGUI() {
    GUILayout.BeginArea(new Rect(0, 2, position.width, position.height - 20));
    scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

    BoltAssetEditorGUI.Header("network", "Replication");
    Replication();

    BoltAssetEditorGUI.Header("connection", "Connection");
    Connection();

    BoltAssetEditorGUI.Header("latency", "Latency Simulation");
    Simulation();

    BoltAssetEditorGUI.Header("settings", "Miscellaneous");
    Miscellaneous();

    BoltAssetEditorGUI.Header("console", "Console");
    Console();

    GUILayout.Space(4);

    EditorGUILayout.EndScrollView();
    GUILayout.EndArea();

    Rect r = new Rect(0, position.height - 20, position.width, 20);

    GUILayout.BeginArea(r);
    BoltAssetEditorGUI.Footer(r);
    GUILayout.EndArea();

    if (GUI.changed) {
      Save();
    }
  }

  void Save() {
    EditorUtility.SetDirty(BoltRuntimeSettings.instance);
    AssetDatabase.SaveAssets();
  }
}
