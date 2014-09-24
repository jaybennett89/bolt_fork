using UnityEngine;
using System.Collections;
using UnityEditor;
using Bolt.Compiler;
using System.Linq;
using System;

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

    if (GUI.changed) {
      Save();
    }
  }

  void Editor() {
    GUILayout.Space(4);

    if ((Selected is AssetDefinition) && (ReferenceEquals(Selected, SelectedAsset) != null)) {
      SelectedAsset = (AssetDefinition)Selected;
    }

    if (SelectedAsset != null) {
      if (SelectedAsset is StateDefinition) {
        EditState((StateDefinition)SelectedAsset);
      }
    }
  }

  void EditState(StateDefinition state) {
    EditHeader(state, BoltEditorGUI.StateHeaderStyle, BoltEditorGUI.StateHeaderColor, () => {
      // separator
      GUILayout.Label(":", BoltEditorGUI.InheritanceSeparatorStyle, GUILayout.ExpandWidth(false));

      // inheritnace
      state.ParentGuid = BoltEditorGUI.AssetPopup(Project.States.Cast<AssetDefinition>(), state.ParentGuid, new Guid[] { });
    });

    BoltEditorGUI.AddButton("Properties", state.Properties, () => new PropertyDefinitionStateAssetSettings());
  }

  void EditHeader(AssetDefinition asset, GUIStyle style, Color color, Action action) {
    GUI.color = color;
    GUILayout.BeginHorizontal(style);
    GUI.color = Color.white;

    // edit asset name
    asset.Name = EditorGUILayout.TextField(asset.Name);

    // remaining header
    action();

    GUILayout.EndHorizontal();
  }
}
