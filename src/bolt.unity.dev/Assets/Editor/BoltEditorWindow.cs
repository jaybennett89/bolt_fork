using UnityEngine;
using System.Collections;
using UnityEditor;
using Bolt.Compiler;
using System.Linq;
using System;
using System.Collections.Generic;

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
    GUILayout.Space(5);

    if ((Selected is AssetDefinition) && (ReferenceEquals(Selected, SelectedAsset) != null)) {
      SelectedAsset = (AssetDefinition)Selected;
    }

    if (SelectedAsset != null) {
      if (SelectedAsset is StateDefinition) {
        EditState((StateDefinition)SelectedAsset);
      }

      if (SelectedAsset is StructDefinition) {
        EditStruct((StructDefinition)SelectedAsset);
      }
    }
  }

  void EditState(StateDefinition def) {
    EditHeader(def, BoltEditorGUI.StateHeaderStyle, BoltEditorGUI.StateHeaderColor, () => {
      // separator
      GUILayout.Label(":", BoltEditorGUI.InheritanceSeparatorStyle, GUILayout.ExpandWidth(false));

      // inheritnace
      def.ParentGuid = BoltEditorGUI.AssetPopup(Project.States.Cast<AssetDefinition>(), def.ParentGuid, new Guid[] { });
    });

    // add button
    BoltEditorGUI.AddButton("Properties", def.Properties, () => new PropertyDefinitionStateAssetSettings());

    // list properties
    EditPropertyList(def, def.Properties);
  }

  void EditStruct(StructDefinition def) {
    EditHeader(def, BoltEditorGUI.StateHeaderStyle, BoltEditorGUI.StateHeaderColor, () => {

    });

    // add button
    BoltEditorGUI.AddButton("Properties", def.Properties, () => new PropertyDefinitionStateAssetSettings());

    // list properties
    EditPropertyList(def, def.Properties);
  }

  void EditHeader(AssetDefinition def, GUIStyle style, Color color, Action action) {
    GUI.color = color;
    GUILayout.BeginHorizontal(style);
    GUI.color = Color.white;

    // edit asset name
    def.Name = EditorGUILayout.TextField(def.Name);

    // remaining header
    action();

    GUILayout.EndHorizontal();
  }

  void EditPropertyList(AssetDefinition def, List<PropertyDefinition> list) {
    for (int i = 0; i < list.Count; ++i) {
      EditProperty(def, list[i]);
    }

    // move nudged property
    for (int i = 0; i < list.Count; ++i) {
      switch (list[i].Nudge) {
        case -1:
          if (i > 0) {
            var a = list[i];
            var b = list[i - 1];

            list[i] = b;
            list[i - 1] = a;
          }
          break;

        case +1:
          if (i + 1 < list.Count) {
            var a = list[i];
            var b = list[i + 1];

            list[i] = b;
            list[i + 1] = a;
          }
          break;
      }
    }

    // remove deleted property
    for (int i = 0; i < list.Count; ++i) {
      if (list[i].Deleted) {
        // remove 
        list.RemoveAt(i);

        // rewind index
        i -= 1;
      }
    }
  }

  void EditProperty(AssetDefinition def, PropertyDefinition p) {
    EditorGUILayout.BeginHorizontal(BoltEditorGUI.ParameterBackgroundStyle);

    // edit name
    p.Name = EditorGUILayout.TextField(p.Name);

    // edit property type
    BoltEditorGUI.PropertyTypePopup(def, p);

    if ((Event.current.modifiers & EventModifiers.Control) == EventModifiers.Control) {
      if (BoltEditorGUI.IconButton("delete")) {
        p.Deleted = true;
      }
    }
    else {
      if (BoltEditorGUI.IconButton("settings2", p.Expanded)) {
        p.Expanded = !p.Expanded;
      }
    }

    EditorGUILayout.EndHorizontal();
  }
}
