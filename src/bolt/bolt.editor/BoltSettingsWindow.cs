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

  void Footer () {
    var version = Assembly.GetExecutingAssembly().GetName().Version;
    var uncompiledCount = EditorPrefs.GetInt("BOLT_UNCOMPILED_COUNT", 0);

    GUILayout.BeginHorizontal();

    // version
    GUILayout.Label(string.Format("{0} ({1})", version, BoltCore.isDebugMode ? "DEBUG" : "RELEASE"), EditorStyles.miniLabel);
    GUILayout.FlexibleSpace();

    // uncompiled
    GUILayout.Label(string.Format("Uncompiled Assets: {0}", uncompiledCount), EditorStyles.miniLabel);

    if (GUILayout.Button("Compile", EditorStyles.miniButton)) {
      BoltUserAssemblyCompiler.Run();
    }

    GUILayout.EndHorizontal();
  }

  void Network () {
    BoltRuntimeSettings settings = BoltRuntimeSettings.instance;

    BoltAssetEditorGUI.Label("FixedUpdate Rate", () => {
      settings._config.framesPerSecond = BoltAssetEditorGUI.IntFieldOverlay(settings._config.framesPerSecond, "Per Second");
    });

    BoltAssetEditorGUI.Label("Packet Interval", () => {
      settings._config.serverSendRate = BoltAssetEditorGUI.IntFieldOverlay(settings._config.serverSendRate, "Frames");
    });

    BoltAssetEditorGUI.Label("Max Connections", () => {
      settings._config.serverConnectionLimit = BoltAssetEditorGUI.IntFieldOverlay(settings._config.serverConnectionLimit, "");
    });

    BoltAssetEditorGUI.Label("Accept Mode", () => {
      settings._config.serverConnectionAcceptMode = (BoltConnectionAcceptMode) EditorGUILayout.EnumPopup(settings._config.serverConnectionAcceptMode);
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
    EditorGUILayout.BeginVertical();
    BoltAssetEditorGUI.Label("Auto Create Console", () => {
      settings._config.autoCreateConsole = EditorGUILayout.Toggle(settings._config.autoCreateConsole);
    });

    if (settings._config.applicationVersion == null) {
      settings._config.applicationVersion = new BoltApplicationVersion(Guid.NewGuid(), 0, 0, 0, 0);
      Save();
    }

    BoltAssetEditorGUI.Label("Application GUID", () => {
      settings._config.applicationVersion.guid = new Guid(EditorGUILayout.TextField(settings._config.applicationVersion.guid.ToString()));
    });

    BoltAssetEditorGUI.Label("Application Version", () => {
      GUIStyle dotStyle = new GUIStyle(EditorStyles.boldLabel);
      dotStyle.contentOffset = new Vector2(0, -3);

      settings._config.applicationVersion.major = BoltAssetEditorGUI.IntFieldOverlay(settings._config.applicationVersion.major, "major");
      GUILayout.Label(".", dotStyle);
      settings._config.applicationVersion.minor = BoltAssetEditorGUI.IntFieldOverlay(settings._config.applicationVersion.minor, "minor");
      GUILayout.Label(".", dotStyle);
      settings._config.applicationVersion.patch = BoltAssetEditorGUI.IntFieldOverlay(settings._config.applicationVersion.patch, "patch");
      GUILayout.Label(".", dotStyle);
      settings._config.applicationVersion.build = BoltAssetEditorGUI.IntFieldOverlay(settings._config.applicationVersion.build, "build");
    });

    BoltAssetEditorGUI.Label("Application Hash", () => {

      byte[] hash = null;

      try {
        hash = BoltRuntimeReflection.GetUserAssemblyHash();
      } catch {
        hash = new byte[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, };
      }

      GUILayout.Label(string.Join("-", hash.Select(x => string.Format("{0:x2}", x)).ToArray()));
    });

    EditorGUILayout.EndVertical();
  }

  void OnGUI () {
    GUILayout.Space(2);

    BoltAssetEditorGUI.Header("network", "Network Settings");
    Network();

    BoltAssetEditorGUI.Header("latency", "Latency Simulation");
    Simulation();

    BoltAssetEditorGUI.Header("settings", "Miscellaneous");
    Miscellaneous();

    GUILayout.BeginArea(new Rect(2, position.height - 18, position.width - 4, 20));
    Footer();
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
