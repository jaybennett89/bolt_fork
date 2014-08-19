using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BoltMenuItems {
  [MenuItem("Assets/Create/Bolt/State")]
  public static void NewStateAsset () {
    BoltEditorUtils.CreateAsset<BoltStateAsset>("BoltState");
  }

  [MenuItem("Assets/Create/Bolt/Event")]
  public static void NewEventAsset () {
    BoltEditorUtils.CreateAsset<BoltEventAsset>("BoltEvent");
  }

  [MenuItem("Assets/Create/Bolt/Command")]
  public static void NewCommandAsset () {
    BoltEditorUtils.CreateAsset<BoltCommandAsset>("BoltCommand");
  }

  [MenuItem("Assets/Create/Bolt/Mecanim")]
  public static void NewMecanimAsset () {
    BoltEditorUtils.CreateAsset<BoltMecanimAsset>("BoltMecanim");
  }

  [MenuItem("Assets/Create/New Scriptable Object Asset")]
  public static void NewScriptableObjectAsset () {
    if (Selection.activeObject is MonoScript) {
      Type type = ((MonoScript) Selection.activeObject).GetClass();
      ScriptableObject obj = ScriptableObject.CreateInstance(type);

      string path = AssetDatabase.GetAssetPath(Selection.activeObject);
      path = Path.GetDirectoryName(path);
      path = path + "/" + type.Name + ".asset";
      path = AssetDatabase.GenerateUniqueAssetPath(path);

      AssetDatabase.CreateAsset(obj, path);

      Selection.activeObject = obj;
    } else {
      Debug.LogError(string.Format("Selected asset must be a MonoScript which contains a ScriptableObject class"));
    }

  }

  [MenuItem("Bolt/Compile")]
  public static void RunCompiler () {
    try {
      BoltUserAssemblyCompiler.Run();
    } catch {

    }
  }

  [MenuItem("Bolt/Install Gizmos")]
  public static void InstallGizmos () {
    BoltEditorUtils.InstallAsset("Assets/Gizmos/BoltEntity Icon.png", BoltEditorUtils.GetResourceBytes("bolt.editor.Resources.BoltEntity Icon.png"));
    BoltEditorUtils.InstallAsset("Assets/Gizmos/BoltEntity Gizmo.png", BoltEditorUtils.GetResourceBytes("bolt.editor.Resources.BoltEntity Gizmo.png"));
  }

  [MenuItem("Bolt/Install Tutorial Assets")]
  public static void InstallTutorialAssets () {
    try {
      Directory.CreateDirectory("Temp");
    } catch { }

    File.WriteAllBytes("Temp/TutorialAssets.unitypackage", BoltEditorUtils.GetResourceBytes("bolt.editor.Resources.TutorialAssets.unitypackage"));
    AssetDatabase.ImportPackage("Temp/TutorialAssets.unitypackage", false);
  }

  [MenuItem("Window/Bolt Remotes")]
  public static void OpenInfoPanel () {
    BoltConnectionsWindow window = EditorWindow.GetWindow<BoltConnectionsWindow>();
    window.title = "Bolt Remotes";
    window.name = "Bolt Remotes";
    window.Show();
  }

  [MenuItem("Window/Bolt Settings")]
  public static void OpenBoltSettings () {
    BoltSettingsWindow window = EditorWindow.GetWindow<BoltSettingsWindow>();
    window.title = "Bolt Settings";
    window.name = "Bolt Settings";
    window.Show();
  }

  [MenuItem("Window/Bolt Scenes")]
  public static void OpenBoltScenes () {
    BoltScenesWindow window = EditorWindow.GetWindow<BoltScenesWindow>();
    window.title = "Bolt Scenes";
    window.name = "Bolt Scenes";
    window.Show();
  }
}
