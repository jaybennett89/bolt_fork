using UnityEditor;

public static class BoltMenuItems {
  [MenuItem("Assets/Create/Bolt/State")]
  public static void NewStateAsset() {
    BoltEditorUtils.CreateAsset<BoltStateAsset>("BoltState");
  }

  [MenuItem("Assets/Create/Bolt/Event")]
  public static void NewEventAsset() {
    BoltEditorUtils.CreateAsset<BoltEventAsset>("BoltEvent");
  }

  [MenuItem("Assets/Create/Bolt/Command")]
  public static void NewCommandAsset() {
    BoltEditorUtils.CreateAsset<BoltCommandAsset>("BoltCommand");
  }

  [MenuItem("Assets/Create/Bolt/Mecanim")]
  public static void NewMecanimAsset() {
    BoltEditorUtils.CreateAsset<BoltMecanimAsset>("BoltMecanim");
  }

  [MenuItem("Bolt/Compile Assets")]
  public static void RunCompiler() {
    try {
      BoltUserAssemblyCompiler.Run();
    }
    catch {

    }
  }

  [MenuItem("Bolt/Install Bolt")]
  public static void Install() {
    int opt = EditorUtility.DisplayDialogComplex("Install Bolt?", "Do you want to install/upgrade Bolt?", "Yes", "Yes (Force)", "No");
    if (opt < 2) {
      BoltInstaller.Run(opt == 1);
    }
  }

  [MenuItem("Window/Bolt Remotes")]
  public static void OpenInfoPanel() {
    BoltConnectionsWindow window = EditorWindow.GetWindow<BoltConnectionsWindow>();
    window.title = "Bolt Remotes";
    window.name = "Bolt Remotes";
    window.Show();
  }

  [MenuItem("Window/Bolt Settings")]
  public static void OpenBoltSettings() {
    BoltSettingsWindow window = EditorWindow.GetWindow<BoltSettingsWindow>();
    window.title = "Bolt Settings";
    window.name = "Bolt Settings";
    window.Show();
  }

  [MenuItem("Window/Bolt Scenes")]
  public static void OpenBoltScenes() {
    BoltScenesWindow window = EditorWindow.GetWindow<BoltScenesWindow>();
    window.title = "Bolt Scenes";
    window.name = "Bolt Scenes";
    window.Show();
  }
}
