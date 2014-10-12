using Bolt.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BoltProjectWindow : BoltWindow {
  [MenuItem("Window/Bolt Engine/Assets", priority = -100)]
  public static void Open() {
    BoltProjectWindow w;

    w = EditorWindow.GetWindow<BoltProjectWindow>();
    w.title = "Bolt Assets";
    w.name = "Bolt Assets";
    w.minSize = new Vector2(150, 200);
    w.Show();
  }

  Vector2 scroll;

  void NewAsset(AssetDefinition def) {
    def.Guid = Guid.NewGuid();
    def.Name = "NewState";

    // add to parent
    ArrayUtility.Add(ref Project.RootFolder.Assets, def);

    // select it
    Select(def);
  }

  new void OnGUI() {
    base.OnGUI();

    scroll = GUILayout.BeginScrollView(scroll, false, false);

    if (HasProject) {
      BoltEditorGUI.HeaderButton("States", "mc_state", () => NewAsset(new StateDefinition()));
      DisplayAssetList(Project.States.Cast<AssetDefinition>());

      BoltEditorGUI.HeaderButton("Structs", "mc_struct", () => NewAsset(new StructDefinition()));
      DisplayAssetList(Project.Structs.Cast<AssetDefinition>());

      BoltEditorGUI.HeaderButton("Commands", "mc_controller", () => NewAsset(new CommandDefinition()));
      DisplayAssetList(Project.Commands.Cast<AssetDefinition>());

      BoltEditorGUI.HeaderButton("Events", "mc_event", () => NewAsset(new EventDefinition()));
      DisplayAssetList(Project.Events.Cast<AssetDefinition>());
    }

    GUILayout.EndScrollView();

    if (GUI.changed) {
      Save();
    }
  }

  void DisplayAssetList(IEnumerable<AssetDefinition> assets) {
    foreach (var a in assets.OrderBy(x => x.Name)) {
      GUILayout.BeginHorizontal();
      GUIStyle style = new GUIStyle(EditorStyles.miniButton);

      if (IsSelected(a)) {
        style.normal.textColor = BoltRuntimeSettings.instance.highlightColor;
      }

      if (GUILayout.Button(a.Name, style)) {
        Select(a);
      }

      GUILayout.EndHorizontal();
    }
  }

  bool IsSelected(object obj) {
    return ReferenceEquals(obj, Selected);
  }

  void Select(INamedAsset asset) {
    Repaints = 10;
    Selected = asset;
    BeginClearFocus();
    BoltEditorGUI.UseEvent();
    BoltEditorWindow.Open();
  }
}
