using UnityEditor;
using UnityEngine;

public static class BoltMenuItems {
  [MenuItem("Assets/Bolt/Compile Assembly")]
  public static void RunCompiler() {
    BoltUserAssemblyCompiler.Run();
  }

  [MenuItem("Assets/Bolt/Update Prefab Database")]
  public static void UpdatePrefabDatabase() {
    if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isPaused || EditorApplication.isUpdating) {
      Debug.LogError("Can't generate prefab database while the editor is playing, paused, updating assets or compiling");
      return;
    }

    BoltCompiler.UpdatePrefabsDatabase();
  }

  [MenuItem("Assets/Bolt/Generate Scene Object Ids")]
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
    BoltEditor.Internal.EditorHousekeeping.AskToSaveSceneAt(System.DateTime.Now.AddSeconds(1));
  }

  [MenuItem("Edit/Install Bolt")]
  public static void Install() {
    if (EditorUtility.DisplayDialog("Install", "Do you want to install Bolt?", "Yes", "No")) {
      BoltInstaller.Run();
    }
  }

  [MenuItem("Window/Bolt/Remotes", priority = 22)]
  public static void OpenInfoPanel() {
    BoltConnectionsWindow window = EditorWindow.GetWindow<BoltConnectionsWindow>();
    window.title = "Bolt Remotes";
    window.name = "Bolt Remotes";
    window.Show();
  }

  [MenuItem("Window/Bolt/Settings", priority = 21)]
  public static void OpenBoltSettings() {
    BoltSettingsWindow window = EditorWindow.GetWindow<BoltSettingsWindow>();
    window.title = "Bolt Settings";
    window.name = "Bolt Settings";
    window.Show();
  }

  [MenuItem("Window/Bolt/Scenes", priority = 20)]
  public static void OpenBoltScenes() {
    BoltScenesWindow window = EditorWindow.GetWindow<BoltScenesWindow>();
    window.title = "Bolt Scenes";
    window.name = "Bolt Scenes";
    window.Show();
  }
}
