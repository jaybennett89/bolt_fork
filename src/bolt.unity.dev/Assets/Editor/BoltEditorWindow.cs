using UnityEngine;
using System.Collections;
using UnityEditor;
using Bolt.Compiler;

public class BoltEditorWindow : BoltWindow {
  [MenuItem("Window/Bolt Editor")]
  public static void Open() {
    BoltEditorWindow w;

    w = EditorWindow.GetWindow<BoltEditorWindow>();
    w.title = "Bolt Editor";
    w.name = "Bolt Editor";
    w.minSize = new Vector2(300, 400);
    w.Show();
  }

  Vector2 scroll;

  void OnGUI() {
    base.OnGUI();

    scroll = GUILayout.BeginScrollView(scroll, false, false);

    if (HasProject) {
      Editor();
    }

    GUILayout.EndScrollView();
  }

  void Editor() {
    if ((Selected is AssetDefinition) && (ReferenceEquals(Selected, SelectedAsset) != null)) {
      SelectedAsset = (AssetDefinition)Selected;
    }

    if (SelectedAsset != null) {

    }
  }
}
