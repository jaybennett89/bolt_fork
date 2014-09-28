using UnityEngine;
using System.Collections;
using UnityEditor;
using Bolt.Compiler;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

public class BoltFilterWindow : BoltWindow {
  static Type[] FindInspectorWindowType() {
    foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
      if (asm.GetName().Name == "UnityEditor") {
        Type t = asm.GetType("UnityEditor.InspectorWindow");

        if (t != null) {
          return new[] { t };
        }
      }
    }

    return new Type[0];
  }

  [MenuItem("Window/Bolt Filters")]
  public static void Open() {
    BoltFilterWindow w;

    w = EditorWindow.GetWindow<BoltFilterWindow>(FindInspectorWindowType());
    w.title = "Bolt Filters";
    w.name = "Bolt Filters";
    w.minSize = new Vector2(200, 400);
    w.Show();
  }

  Vector2 scroll;

  new void OnGUI() {
    base.OnGUI();

    scroll = GUILayout.BeginScrollView(scroll, false, false);

    if (HasProject) {
      EditFilters();
    }

    GUILayout.EndScrollView();

    if (GUI.changed) {
      Save();
    }
  }

  void EditFilters() {
    // always has a value
    if (Project.Filters == null) {
      Project.Filters = new FilterDefinition[32];
      Save();
    }

    // always exactly 32
    if (Project.Filters.Length != 32) {
      Array.Resize(ref Project.Filters, 32);
      Save();
    }

    // always has values
    for (int i = 0; i < Project.Filters.Length; ++i) {
      if (Project.Filters[i] == null) {
        Project.Filters[i] = new FilterDefinition() { Enabled = (i == 0), Index = i, Name = "Filter" + i, Color = BoltEditorGUI.ColorInt(179, 179, 179).ToBoltColor() };
        Save();
      }
    }

    // edit filters
    for (int i = 0; i < Project.Filters.Length; ++i) {
      EditFilter(Project.Filters[i]);
    }
  }

  void EditFilter(FilterDefinition f) {
    EditorGUILayout.BeginHorizontal();

    if (BoltEditorGUI.OnOffButton("ui-check-box".ToContent(), "ui-check-box-uncheck".ToContent(), f.Enabled)) {
      f.Enabled = !f.Enabled;
    }

    EditorGUI.BeginDisabledGroup(f.Enabled == false);
    f.Name = EditorGUILayout.TextField(f.Name);
    f.Color = EditorGUILayout.ColorField(f.Color.ToUnityColor()).ToBoltColor();
    EditorGUI.EndDisabledGroup();

    EditorGUILayout.EndHorizontal();
  }
}
