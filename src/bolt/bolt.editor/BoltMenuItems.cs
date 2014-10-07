using ProtoBuf;
using UnityEditor;
using Bolt.Compiler;

public static class BoltMenuItems {
  [MenuItem("Assets/Compile Bolt Assets")]
  public static void RunCompiler() {
    try {
      BoltUserAssemblyCompiler.Run();
    }
    catch {

    }
  }

  [MenuItem("Edit/Install Bolt")]
  public static void Install() {
    int opt = EditorUtility.DisplayDialogComplex("Install Bolt?", "Do you want to install/upgrade Bolt?", "Yes", "Yes (Force)", "No");
    if (opt < 2) {
      BoltInstaller.Run(opt == 1);
    }
  }

  [MenuItem("Window/Bolt Remotes", priority = 22)]
  public static void OpenInfoPanel() {
    BoltConnectionsWindow window = EditorWindow.GetWindow<BoltConnectionsWindow>();
    window.title = "Bolt Remotes";
    window.name = "Bolt Remotes";
    window.Show();
  }

  [MenuItem("Window/Bolt Settings", priority = 21)]
  public static void OpenBoltSettings() {
    BoltSettingsWindow window = EditorWindow.GetWindow<BoltSettingsWindow>();
    window.title = "Bolt Settings";
    window.name = "Bolt Settings";
    window.Show();
  }

  [MenuItem("Window/Bolt Scenes", priority = 20)]
  public static void OpenBoltScenes() {
    BoltScenesWindow window = EditorWindow.GetWindow<BoltScenesWindow>();
    window.title = "Bolt Scenes";
    window.name = "Bolt Scenes";
    window.Show();
  }
}
