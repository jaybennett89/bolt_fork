using ProtoBuf;
using UnityEditor;
using Bolt.Compiler;
using UnityEngine;

public static class BoltMenuItems {

  [MenuItem("Assets/Bolt Engine/Compile Assets (All)")]
  public static void RunCompiler() {
    try {
      BoltUserAssemblyCompiler.Run(true);
    }
    catch {

    }
  }

  [MenuItem("Assets/Bolt Engine/Compile Assets (Code Only)")]
  public static void RunCompilerProjectOnly() {
    try {
      BoltUserAssemblyCompiler.Run(false);
    }
    catch {

    }
  }

  [MenuItem("Assets/Bolt Engine/Generate Scene Object Ids")]
  public static void GenerateSceneObjectGuids() {
    if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isPaused || EditorApplication.isUpdating) {
      Debug.LogError("Can't generate scene guids while the editor is playing, paused, updating assets or compiling");
      return;
    }

    foreach (BoltEntity en in GameObject.FindObjectsOfType<BoltEntity>()) {
      en.ModifySettings().sceneId = Bolt.UniqueId.New();
      EditorUtility.SetDirty(en);
      EditorUtility.SetDirty(en.gameObject);
      Debug.Log(string.Format("Assigned new scene id to {0}", en));
    }

    // save scene
    Bolt.Editor.Internal.EditorHousekeeping.AskToSaveSceneAt = System.DateTime.Now.AddSeconds(1);
  }

  [MenuItem("Edit/Install Bolt")]
  public static void Install() {
    if (EditorUtility.DisplayDialog("Install", "Do you want to install Bolt?", "Yes", "No")) {
      BoltInstaller.Run();
    }
  }


  //[MenuItem("Edit/Upgrade Bolt")]
  //public static void Upgrade() {
  //  if (EditorUtility.DisplayDialog("Upgrade", "Do you want to upgrade Bolt? This will shut down Unity during the upgrade process", "Yes", "No")) {
  //    var package = EditorUtility.OpenFilePanel("Select Package", ".", "unitypackage");
  //    Debug.Log(string.Format("Importing {0}", package));
  //    AssetDatabase.ImportPackage(package, false);
  //    EditorPrefs.SetBool(UPGRADE_FLAG, true);
  //    EditorUtility.DisplayDialog("", "Click OK to restart Unity", "Ok");
  //    EditorApplication.Exit(0);
  //  }
  //}

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
