using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Process = System.Diagnostics.Process;

public class BoltScenesWindow : EditorWindow {

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
      } else {
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
      } else {
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
    name = title = "Bolt Scenes";
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

    } else {
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

  void Settings_ServerPort () {
    BoltRuntimeSettings settings = BoltRuntimeSettings.instance;

    GUILayout.BeginHorizontal();

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

    GUILayout.EndHorizontal();

  }

  void Settings () {
    BoltRuntimeSettings settings = BoltRuntimeSettings.instance;
    GUILayout.BeginVertical();
    Settings_ServerPort();
    settings.debugEditorMode = (BoltEditorStartMode) EditorGUILayout.EnumPopup("Editor Mode", settings.debugEditorMode);
    settings.debugClientCount = EditorGUILayout.IntField("Clients", settings.debugClientCount);
    GUILayout.EndVertical();
  }

  Vector2 sceneScrollPosition;

  void Scenes () {
    sceneScrollPosition = GUILayout.BeginScrollView(sceneScrollPosition, GUILayout.MaxHeight(300));

    foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
      if (scene.enabled) {
        var sceneName = Path.GetFileNameWithoutExtension(scene.path);
        var isCurrent = EditorApplication.currentScene == scene.path;
        GUIStyle sceneStyle = isCurrent ? "TE NodeBoxSelected" : "TE NodeBox";
        sceneStyle.padding = new RectOffset(5, 5, 5, 5);

        GUILayout.BeginHorizontal(sceneStyle, GUILayout.Height(22));
        GUILayout.Label(sceneName);

        // Scene Edit Button

        EditorGUI.BeginDisabledGroup(isCurrent);

        if (GUILayout.Button("Edit", EditorStyles.miniButton, GUILayout.Width(50))) {
          EditorApplication.OpenScene(scene.path);
        }

        EditorGUI.EndDisabledGroup();

        // Scene Start Button

        if (BoltEditorUtils.hasPro) {
          BoltRuntimeSettings settings = BoltRuntimeSettings.instance;

          if (GUILayout.Button("Start", EditorStyles.miniButton, GUILayout.Width(50))) {
            if (EditorApplication.SaveCurrentSceneIfUserWantsTo()) {
              settings.debugStartMapName = sceneName;

              // save asset
              EditorUtility.SetDirty(settings);
              AssetDatabase.SaveAssets();

              // set stage
              SetStage(STAGE_COMPILE_BOLT);
            }
          }
        }

        GUILayout.EndHorizontal();
      }
    }
    GUILayout.EndScrollView();
  }

  void OnGUI () {
    GUILayout.Space(2);

    if (BoltEditorUtils.hasPro) {
      BoltAssetEditorGUI.Header("play", "Debug Start Settings");
      Settings();
    }

    BoltAssetEditorGUI.Header("scenes", "Scenes");
    GUILayout.Space(1);
    Scenes();

    if (GUI.changed) {
      EditorUtility.SetDirty(BoltRuntimeSettings.instance);
      AssetDatabase.SaveAssets();
    }
  }
}
