using Bolt.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    def.Name = "New" + def.GetType().Name.Replace("Definition", "");

    // add to parent
    ArrayUtility.Add(ref Project.RootFolder.Assets, def);

    // select it
    Select(def);
  }

  new void OnGUI() {
    base.OnGUI();

    GUILayout.BeginArea(new Rect(0, 0, position.width, position.height - 16));

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

      if (BoltEditorGUI.IsRightClick) {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("New State"), false, () => NewAsset(new StateDefinition()));
        menu.AddItem(new GUIContent("New Struct"), false, () => NewAsset(new StructDefinition()));
        menu.AddItem(new GUIContent("New Event"), false, () => NewAsset(new EventDefinition()));
        menu.AddItem(new GUIContent("New Command"), false, () => NewAsset(new CommandDefinition()));
        menu.ShowAsContext();
      }
    }

    if (GUI.changed) {
      Save();
    }

    ClearAllFocus();

    GUILayout.EndScrollView();
    GUILayout.EndArea();

    GUILayout.BeginArea(new Rect(0, position.height - 16, position.width, 16));
    Footer();
    GUILayout.EndArea();
  }

  void Footer() {
    var version = Assembly.GetExecutingAssembly().GetName().Version;
    GUILayout.Label(string.Format("{0} ({1})", version, BoltCore.isDebugMode ? "DEBUG" : "RELEASE"), EditorStyles.miniLabel);
  }

  void DisplayAssetList(IEnumerable<AssetDefinition> assets) {
    bool deleteMode = (Event.current.modifiers & EventModifiers.Control) == EventModifiers.Control;

    foreach (var a in assets.OrderBy(x => x.Name)) {
      GUILayout.BeginHorizontal();
      GUIStyle style = new GUIStyle(EditorStyles.miniButton);
      style.alignment = TextAnchor.MiddleLeft;

      if (IsSelected(a)) {
        style.normal.textColor = BoltRuntimeSettings.instance.highlightColor;
      }

      GUILayout.Space(5);

      if (GUILayout.Button(new GUIContent(a.Name), style)) {
        if (deleteMode) {
          a.Deleted = true;

          if (IsSelected(a)) {
            Select(null);
          }
        }
        else {
          Select(a);
        }
      }

      if (deleteMode) {
        Rect r = GUILayoutUtility.GetLastRect();
        r.xMin = r.xMax - 13;
        r.xMax = r.xMax - 2;
        r.yMin = r.yMin + 2;
        r.yMax = r.yMax - 2;

        GUI.color = BoltRuntimeSettings.instance.highlightColor;
        GUI.DrawTexture(r, BoltEditorGUI.LoadIcon("mc_minus"));
        GUI.color = Color.white;
      }

      GUILayout.EndHorizontal();
    }


    for (int i = 0; i < Project.RootFolder.Assets.Length; ++i) {
      if (Project.RootFolder.Assets[i].Deleted) {
        // remove deleted assets
        ArrayUtility.RemoveAt(ref Project.RootFolder.Assets, i);

        // decrement index
        i -= 1;

        // save project
        Save();
      }
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

    if (BoltRuntimeSettings.instance.autoSwitchToEditor) {
      BoltEditorWindow.Open();
    }
  }
}
