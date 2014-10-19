using ProtoBuf;
using UnityEditor;
using Bolt.Compiler;
using UnityEngine;

//[InitializeOnLoad]
public static class BoltMenuItems {

  //const string UPGRADE_FLAG = "BOLT_UPGRADE";

  //static BoltMenuItems() {
  //  if (EditorPrefs.GetBool(UPGRADE_FLAG, false)) {
  //    EditorPrefs.SetBool(UPGRADE_FLAG, false);
  //    RunCompiler();
  //  }
  //}

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
