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

  public const string COMPILE_SETTING = "BOLT_COMPILE";

  const int STAGE_NONE = 0;
  const int STAGE_COMPILE_BOLT = 5;
  const int STAGE_COMPILE_BOLT_WAIT = 6;
  const int STAGE_COMPILE_PLAYER = 1;
  const int STAGE_START_PLAYERS = 2;
  const int STAGE_START_EDITOR = 3;
  const int STAGE_PLAYING = 4;

  const string DEBUGSTART_STAGE = "BOLT_DEBUGSTART_STAGE";
  const string DEBUGSTART_RESTORESCENE = "BOLT_DEBUGSTART_RESTORESCENE";

  float _lastRepaint;

  bool isOSX {
    get { return Application.platform == RuntimePlatform.OSXEditor; }
  }

  bool isWindows {
    get { return Application.platform == RuntimePlatform.WindowsEditor; }
  }

  string debugScene {
    get { return "Assets/bolt/scenes/BoltDebugScene.unity"; }
  }

  string debugSceneNonPro {
    get { return "Assets/bolt/scenes/BoltDebugNonProScene.unity"; }
  }

  string[] scenes {
    get { return (new[] { debugScene }).Concat(EditorBuildSettings.scenes.Select(x => x.path)).ToArray(); }
  }

  string playerPath {
    get {
      if (isOSX) {
        return "Bolt_DebugStart_Build/Bolt_DebugStart_Build";
      }

      return "Bolt_DebugStart_Build\\Bolt_DebugStart_Build.exe";
    }
  }

  string playerPathExecutable {
    get {
      if (isOSX) {
        string[] paths = new string[] {
          playerPath + ".app/Contents/MacOS/" + PlayerSettings.productName,
          playerPath + "/Contents/MacOS/" + PlayerSettings.productName,
          playerPath + "/Contents/MacOS/Bolt_DebugStart_Build",
          playerPath + ".app/Contents/MacOS/Bolt_DebugStart_Build"
        };

        for (int i = 0; i < paths.Length; ++i) {
          if (File.Exists(paths[i])) return paths[i];
        }

        throw new BoltException("Could not find executable at any of the following paths: ", paths.Join(", "));
      }

      return playerPath;
    }
  }

  BuildTarget buildTarget {
    get {
      if (isOSX) {
        return BuildTarget.StandaloneOSXIntel;
      }

      return BuildTarget.StandaloneWindows;
    }
  }

  BuildOptions buildOptions {
    get { return BuildOptions.Development | BuildOptions.AllowDebugging; }
  }

  void BuildPlayer () {
    try {
      if (BoltRuntimeSettings.instance.debugClientCount == 0 && BoltRuntimeSettings.instance.debugEditorMode == BoltEditorStartMode.Server) {
        EditorPrefs.SetInt(DEBUGSTART_STAGE, STAGE_START_PLAYERS);
        return;
      }
      string path =
        BoltEditorUtils.MakePath(
          Path.GetDirectoryName(Application.dataPath),
          isOSX ? playerPath : Path.GetDirectoryName(playerPath)
        );

      try {
        Directory.CreateDirectory(path);
      } catch (Exception exn) {
        Debug.LogException(exn);
      }

      string result;

      result = BuildPipeline.BuildPlayer(scenes, playerPath, buildTarget, buildOptions);
      result = (result ?? "").Trim();

      if (result.Length == 0) {
        EditorPrefs.SetInt(DEBUGSTART_STAGE, STAGE_START_PLAYERS);
      }
      else {
        EditorPrefs.SetInt(DEBUGSTART_STAGE, STAGE_NONE);
      }
    } catch {
      EditorPrefs.SetInt(DEBUGSTART_STAGE, STAGE_NONE);
      throw;
    }
  }

  void PositionWindowsOnOSX () {
    if (isOSX && (BoltRuntimeSettings.instance.debugEditorMode == BoltEditorStartMode.None)) {
      Process p = new Process();
      p.StartInfo.FileName = "osascript";
      p.StartInfo.Arguments =

@"-e 'tell application """ + UnityEditor.PlayerSettings.productName + @"""
	activate
end tell'";

      p.Start();
    }
  }

  void StartPlayers () {
    try {
      int clientCount = BoltRuntimeSettings.instance.debugClientCount;

      // starting server player

      if (BoltRuntimeSettings.instance.debugEditorMode == BoltEditorStartMode.Client) {
        clientCount -= 1;
        Process p = new Process();
        p.StartInfo.FileName = playerPathExecutable;
        p.StartInfo.Arguments = "--bolt-debugstart-server";
        p.Start();
      }

      // start client players
      for (int i = 0; i < clientCount; ++i) {
        Process p = new Process();
        p.StartInfo.FileName = playerPathExecutable;
        p.StartInfo.Arguments = "--bolt-debugstart-client --bolt-window-index-" + i;
        p.Start();
      }

      PositionWindowsOnOSX();


    } finally {
      EditorPrefs.SetInt(DEBUGSTART_STAGE, STAGE_START_EDITOR);
    }
  }

  void StopPlayers () {
    if (Application.platform == RuntimePlatform.WindowsEditor) {
      try {
        foreach (Process p in Process.GetProcesses()) {
          try {
            if (p.ProcessName == "Bolt_DebugStart_Build") {
              p.Kill();
            }
          } catch { }
        }
      } catch { }
    }

    if (Application.platform == RuntimePlatform.OSXEditor) {
      try {
        foreach (Process p in Process.GetProcesses()) {
          try {
            if (p.ProcessName == PlayerSettings.productName) {
              p.Kill();
            }

            if (p.ProcessName == "Bolt_DebugStart_Build") {
              p.Kill();
            }
          } catch { }
        }
      } catch { }
    }
  }

  void LoadAndStartScene (bool pro) {
    EditorPrefs.SetString(DEBUGSTART_RESTORESCENE, EditorApplication.currentScene);
    EditorPrefs.SetInt(DEBUGSTART_STAGE, STAGE_PLAYING);

    if (EditorApplication.OpenScene(pro ? debugScene : debugSceneNonPro)) {
      EditorApplication.isPlaying = true;
    }
  }

  void StartEditor () {
    try {
      if (BoltEditorUtils.hasPro == false) {
        LoadAndStartScene(false);
      }
      else {
        switch (BoltRuntimeSettings.instance.debugEditorMode) {
          case BoltEditorStartMode.Client:
          case BoltEditorStartMode.Server:
            LoadAndStartScene(true);
            break;

          case BoltEditorStartMode.None:
            EditorPrefs.SetInt(DEBUGSTART_STAGE, STAGE_NONE);
            break;
        }
      }
    } catch {
      EditorPrefs.SetInt(DEBUGSTART_STAGE, STAGE_NONE);
      throw;
    }
  }

  void StopEditor () {
    if (EditorApplication.isPlaying == false && EditorApplication.isPlayingOrWillChangePlaymode == false) {
      // reload scene
      if (EditorPrefs.HasKey(DEBUGSTART_RESTORESCENE)) {
        EditorApplication.OpenScene(EditorPrefs.GetString(DEBUGSTART_RESTORESCENE));
        EditorPrefs.DeleteKey(DEBUGSTART_RESTORESCENE);
      }

      // kill players
      StopPlayers();

      // reset stage state
      EditorPrefs.SetInt(DEBUGSTART_STAGE, STAGE_NONE);
    }
  }

  void OnEnable () {
    name = title = "Bolt Settings";
    _lastRepaint = 0f;
  }

  void Update () {
    if (_lastRepaint + 0.1f < Time.realtimeSinceStartup) {
      _lastRepaint = Time.realtimeSinceStartup;
      Repaint();
    }

    switch (EditorPrefs.GetInt(DEBUGSTART_STAGE)) {
      case STAGE_COMPILE_BOLT:
        CompileBolt();
        break;

      case STAGE_COMPILE_BOLT_WAIT:
        CompileBoltWait();
        break;

      case STAGE_COMPILE_PLAYER:
        BuildPlayer();
        break;

      case STAGE_START_PLAYERS:
        StartPlayers();
        break;

      case STAGE_START_EDITOR:
        StartEditor();
        break;

      case STAGE_PLAYING:
        StopEditor();
        break;
    }
  }

  void SetStage (int stage) {
    EditorPrefs.SetInt(DEBUGSTART_STAGE, stage);
  }

  void CompileBolt () {
    if (EditorPrefs.GetBool(COMPILE_SETTING)) {
      SetStage(STAGE_COMPILE_BOLT_WAIT);
      BoltUserAssemblyCompiler.Run();

    }
    else {
      SetStage(STAGE_COMPILE_PLAYER);
    }
  }

  void CompileBoltWait () {
    if (EditorPrefs.GetBool(COMPILE_SETTING) == false && EditorApplication.isCompiling == false) {
      SetStage(STAGE_COMPILE_PLAYER);
    }
  }

  void CompileButton () {
    if (EditorPrefs.GetBool(COMPILE_SETTING)) {
      GUI.color = Color.red;
    }

    GUI.color = Color.white;
  }

  int _selectedTab = 0;
  STuple<string, Action<Rect>>[] _tabs;

  void Toolbar (Rect r) {
    GUILayout.BeginHorizontal();

    for (int i = 0; i < _tabs.Length; ++i) {
      GUIContent content;

      if (i == _selectedTab) {
        content = new GUIContent(_tabs[i].item0, BoltAssetEditorGUI.Icon("BoltActiveTab"));
      }
      else {
        content = new GUIContent(_tabs[i].item0, BoltAssetEditorGUI.Icon("BoltEmptyIcon"));
      }

      if (GUILayout.Button(content, new GUIStyle("toolbarbutton"))) {
        _selectedTab = i;
      }
    }

    GUILayout.EndHorizontal();
    _selectedTab = Mathf.Clamp(_selectedTab, 0, _tabs.Length - 1);
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

  void Scenes (Rect r) {
    BoltRuntimeSettings settings = BoltRuntimeSettings.instance;

    if (BoltEditorUtils.hasPro) {
      GUILayout.BeginVertical();
      EditorGUILayout.BeginHorizontal();
      settings.debugStartPort = EditorGUILayout.IntField("Server Port", settings.debugStartPort);

      if (GUILayout.Button("Refresh", EditorStyles.miniButton)) {
        Socket sc = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        sc.Bind(new IPEndPoint(IPAddress.Any, 0));

        settings.debugStartPort = (sc.LocalEndPoint as IPEndPoint).Port;

        try {
          sc.Shutdown(SocketShutdown.Both);
          sc.Close();
        } catch { }

        EditorUtility.SetDirty(settings);
      }

      EditorGUILayout.EndHorizontal();

      settings.debugEditorMode = (BoltEditorStartMode) EditorGUILayout.EnumPopup("Editor Mode", settings.debugEditorMode);
      settings.debugClientCount = EditorGUILayout.IntField("Clients", settings.debugClientCount);
      GUILayout.EndVertical();
    }

    GUILayout.BeginVertical();

    GUIStyle sceneStyle;
    sceneStyle = new GUIStyle("TL LogicBar 1");
    sceneStyle.padding = new RectOffset(2, 2, 2, 2);
    sceneStyle.margin = new RectOffset(0, 0, 0, 0);

    foreach (var scene in EditorBuildSettings.scenes) {
      var sceneName = Path.GetFileNameWithoutExtension(scene.path);
      if (scene.enabled) {
        BoltAssetEditorGUI.EditBox(sceneStyle, () => {
          GUILayout.BeginHorizontal();
          GUILayout.Label(sceneName, GUILayout.MinWidth(100));

          if (GUILayout.Button("Edit", EditorStyles.miniButton, GUILayout.Width(50))) {
            EditorApplication.OpenScene(scene.path);
          }

          if (BoltEditorUtils.hasPro) {
            EditorGUI.BeginDisabledGroup(EditorApplication.isCompiling);

            if (GUILayout.Button("Start", EditorStyles.miniButton, GUILayout.Width(50))) {
              EditorApplication.SaveCurrentSceneIfUserWantsTo();
              settings.debugStartMapName = sceneName;

              // save asset
              EditorUtility.SetDirty(settings);
              AssetDatabase.SaveAssets();

              // set stage
              SetStage(STAGE_COMPILE_BOLT);
            }

            EditorGUI.EndDisabledGroup();
          }

          GUILayout.EndHorizontal();
        });
      }
    }

    GUILayout.EndVertical();
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

    BoltAssetEditorGUI.Label("Dejitter Delay", () => {
      settings._config.serverDejitterDelayMin = BoltAssetEditorGUI.IntFieldOverlay(settings._config.serverDejitterDelayMin, "Min");
      settings._config.serverDejitterDelay = BoltAssetEditorGUI.IntFieldOverlay(settings._config.serverDejitterDelay, "Frames");
      settings._config.serverDejitterDelayMax = BoltAssetEditorGUI.IntFieldOverlay(settings._config.serverDejitterDelayMax, "Max");
    });
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

  void OnGUI () {
    GUILayout.Space(2);

    BoltAssetEditorGUI.Header("Network Settings");
    Network();

    BoltAssetEditorGUI.Header("Latency Simulation");
    Simulation();

    GUILayout.BeginArea(new Rect(2, position.height - 18, position.width - 4, 20));
    Footer();
    GUILayout.EndArea();
  }
}
