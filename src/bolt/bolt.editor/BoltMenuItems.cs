﻿using ProtoBuf;
using UnityEditor;
using Bolt.Compiler;

public static class BoltMenuItems {
  [MenuItem("Assets/Compile Bolt Assets (All)")]
  public static void RunCompiler() {
    try {
      BoltUserAssemblyCompiler.Run(true);
    }
    catch {

    }
  }

  [MenuItem("Assets/Compile Bolt Assets (Code Only)")]
  public static void RunCompilerProjectOnly() {
    try {
      BoltUserAssemblyCompiler.Run(false);
    }
    catch {

    }
  } 
  [MenuItem("Edit/Install Bolt")]
  public static void Install() {
    if (EditorUtility.DisplayDialog("Install Bolt?", "Do you want to install/upgrade Bolt?", "Yes", "No")) {
      BoltInstaller.Run();
    }
  }

  [MenuItem("Window/Bolt Engine/Remotes", priority = 22)]
  public static void OpenInfoPanel() {
    BoltConnectionsWindow window = EditorWindow.GetWindow<BoltConnectionsWindow>();
    window.title = "Bolt Remotes";
    window.name = "Bolt Remotes";
    window.Show();
  }

  [MenuItem("Window/Bolt Engine/Settings", priority = 21)]
  public static void OpenBoltSettings() {
    BoltSettingsWindow window = EditorWindow.GetWindow<BoltSettingsWindow>();
    window.title = "Bolt Settings";
    window.name = "Bolt Settings";
    window.Show();
  }

  [MenuItem("Window/Bolt Engine/Scenes", priority = 20)]
  public static void OpenBoltScenes() {
    BoltScenesWindow window = EditorWindow.GetWindow<BoltScenesWindow>();
    window.title = "Bolt Scenes";
    window.name = "Bolt Scenes";
    window.Show();
  }
}
