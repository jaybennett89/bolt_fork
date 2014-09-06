using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Process = System.Diagnostics.Process;

public class BoltSettingsWindow : EditorWindow {
  float _lastRepaint;

  void OnEnable () {
    name = title = "Bolt Settings";
    _lastRepaint = 0f;
  }

  void Update () {
    if (_lastRepaint + 0.1f < Time.realtimeSinceStartup) {
      _lastRepaint = Time.realtimeSinceStartup;
      Repaint();
    }
  }

  void Footer (Rect r) {
    var version = Assembly.GetExecutingAssembly().GetName().Version;
    var uncompiledCount = EditorPrefs.GetInt("BOLT_UNCOMPILED_COUNT", 0);

    GUIStyle bg;

    bg = new GUIStyle(GUIStyle.none);
    bg.normal.background = EditorGUIUtility.whiteTexture;

    GUI.color = new Color(0.25f, 0.25f, 0.25f);
    GUILayout.BeginHorizontal(bg);
    GUI.color = Color.white;

    // version
    GUILayout.Label(string.Format("{0} ({1})", version, BoltCore.isDebugMode ? "DEBUG" : "RELEASE"), EditorStyles.miniLabel);
    GUILayout.FlexibleSpace();

    // uncompiled
    GUILayout.Label(string.Format("Uncompiled Assets: {0}", uncompiledCount), EditorStyles.miniLabel);
    
    // compile button
    GUIStyle compileButton = new GUIStyle(EditorStyles.miniButton);
    compileButton.normal.textColor = 
      uncompiledCount == 0 
        ? compileButton.normal.textColor 
        : BoltAssetEditorGUI.lightBlue;

    if (GUILayout.Button("Compile", compileButton)) {
      BoltUserAssemblyCompiler.Run();
    }

    GUILayout.EndHorizontal();
  }

  void Replication () {
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

    settings._config.clientSendRate = settings._config.serverSendRate;
    settings._config.clientDejitterDelay = settings._config.serverDejitterDelay;
    settings._config.clientDejitterDelayMin = settings._config.serverDejitterDelayMin;
    settings._config.clientDejitterDelayMax = settings._config.serverDejitterDelayMax;
  }

  void Connection () {
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
      settings._config.serverConnectionAcceptMode = (BoltConnectionAcceptMode) EditorGUILayout.EnumPopup(settings._config.serverConnectionAcceptMode);
    });

    EditorGUI.BeginDisabledGroup(settings._config.serverConnectionAcceptMode != BoltConnectionAcceptMode.Manual);

    BoltAssetEditorGUI.Label("Accept Token Size", () => {
      settings._config.connectionTokenSize = Mathf.Clamp(BoltAssetEditorGUI.IntFieldOverlay(settings._config.connectionTokenSize, "Bytes"), 0, UdpKit.UdpSocket.MaxConnectionTokenSize);
    });

    EditorGUI.EndDisabledGroup();
  }

  void Simulation () {
    BoltRuntimeSettings settings = BoltRuntimeSettings.instance;

    EditorGUILayout.BeginVertical();

    if (BoltCore.isDebugMode == false) {
      EditorGUILayout.HelpBox("Bolt is compiled in release mode, these settings have no effectr", MessageType.Warning);
    }

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
      settings._config.simulatedRandomFunction = (BoltRandomFunction) EditorGUILayout.EnumPopup(settings._config.simulatedRandomFunction);
    });

    EditorGUILayout.EndVertical();
  }

  void Miscellaneous () {
    BoltRuntimeSettings settings = BoltRuntimeSettings.instance;

    BoltAssetEditorGUI.Label("Compiler Warn Level", () => {
      settings.compilationWarnLevel = EditorGUILayout.IntField(settings.compilationWarnLevel);
      settings.compilationWarnLevel = Mathf.Clamp(settings.compilationWarnLevel, 0, 4);
    });

    BoltAssetEditorGUI.Label("Use Unique Ids", () => {
      settings._config.globalUniqueIds = EditorGUILayout.Toggle(settings._config.globalUniqueIds);
    });

    BoltAssetEditorGUI.Label("Log Targets", () => {
      settings._config.logTargets = (BoltConfigLogTargets) EditorGUILayout.EnumMaskField(settings._config.logTargets);
    });

    if (settings._config.applicationGuid == null || settings._config.applicationGuid.Length != 16) {
      settings._config.applicationGuid = Guid.NewGuid().ToByteArray();
      Save();
    }

    BoltAssetEditorGUI.Label("Application Identifier", () => {
      Guid g;
      g = new Guid(settings._config.applicationGuid);
      g = new Guid(EditorGUILayout.TextField(g.ToString().ToUpperInvariant()));

      settings._config.applicationGuid = g.ToByteArray();

      if (GUILayout.Button("Generate", EditorStyles.miniButton)) {
        settings._config.applicationGuid = Guid.NewGuid().ToByteArray();
        Save();
      }
    });

    BoltAssetEditorGUI.Label("Assembly Checksum", () => {
      byte[] hash = null;

      try {
        hash = BoltRuntimeReflection.GetUserAssemblyHash();
      } catch {
        hash = new byte[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, };
      }

      GUILayout.Label(string.Join("-", hash.Select(x => string.Format("{0:x2}", x).ToUpperInvariant()).ToArray()));
    });
  }

  void Console () {

    BoltRuntimeSettings settings = BoltRuntimeSettings.instance;

    var consoleEnabled = (settings._config.logTargets & BoltConfigLogTargets.Console) == BoltConfigLogTargets.Console;
    EditorGUI.BeginDisabledGroup(consoleEnabled == false);

    EditorGUILayout.BeginVertical();

    BoltAssetEditorGUI.Label("Toggle Key", () => {
      settings.consoleToggleKey = (KeyCode) EditorGUILayout.EnumPopup(settings.consoleToggleKey);
    });

    BoltAssetEditorGUI.Label("Visible By Default", () => {
      settings.consoleVisibleByDefault = EditorGUILayout.Toggle(settings.consoleVisibleByDefault);
    });

    EditorGUI.EndDisabledGroup();
    EditorGUILayout.EndVertical();
  }

  Vector2 scrollPos = Vector2.zero;

  void OnGUI () {
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

    Rect r = new Rect(0, position.height - 20, position.width, 20);

    GUILayout.EndArea();
    GUILayout.BeginArea(r);
    Footer(r);
    GUILayout.EndArea();

    if (GUI.changed) {
      Save();
    }
  }

  void Save () {
    EditorUtility.SetDirty(BoltRuntimeSettings.instance);
    AssetDatabase.SaveAssets();
  }
}
