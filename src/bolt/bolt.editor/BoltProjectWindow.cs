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

  string addGroup = null;
  AssetDefinition addGroupTo = null;

  Vector2 scroll;

  bool HasGroupSelected {
    get { return !string.IsNullOrEmpty(Project.ActiveGroup) && Project.ActiveGroup != "Everything"; }
  }

  void NewAsset(AssetDefinition def) {
    def.Guid = Guid.NewGuid();
    def.Name = "New" + def.GetType().Name.Replace("Definition", "");

    if (HasGroupSelected) {
      def.Groups.Add(Project.ActiveGroup);
    }

    // add to parent
    ArrayUtility.Add(ref Project.RootFolder.Assets, def);

    // select it
    Select(def);

    // save project
    Save();
  }

  new void OnGUI() {
    base.OnGUI();

    GUILayout.BeginArea(new Rect(0, 0, position.width, position.height - 16));
    scroll = GUILayout.BeginScrollView(scroll, false, false);

    EditorGUILayout.BeginHorizontal();

    var addingGroup = addGroup != null && addGroupTo != null;

    if (addingGroup) {
      GUI.SetNextControlName("BoltProjectWindow_AddGroup");
      addGroup = GUILayout.TextField(addGroup);
      GUI.FocusControl("BoltProjectWindow_AddGroup");

      switch (Event.current.keyCode.ToString()) {
        case "Return":
          addGroupTo.Groups.Add(addGroup);
          addGroup = null;
          addGroupTo = null;
          break;

        case "Escape":
          addGroup = null;
          addGroupTo = null;
          break;
      }
    }
    else {
      EditorGUI.BeginDisabledGroup(Project.Groups.Count() == 0);

      var list = new[] { "Everything" }.Concat(Project.Groups).ToArray();
      var listCounted = new[] { "Everything (" + Project.RootFolder.Assets.Length + ")" }.Concat(Project.Groups.Select(x => x + " (" + Project.RootFolder.Assets.Count(a => a.Groups.Contains(x)) + ")")).ToArray();

      var index = Mathf.Max(0, Array.IndexOf(list, Project.ActiveGroup));
      var selected = EditorGUILayout.Popup(index, listCounted);

      if (Project.ActiveGroup != list[selected]) {
        Project.ActiveGroup = list[selected];
        Save();
      }

      EditorGUI.EndDisabledGroup();
    }

    EditorGUILayout.EndHorizontal();

    if (HasProject) {
      BoltEditorGUI.HeaderButton("States", "mc_state", () => NewAsset(new StateDefinition()));
      DisplayAssetList(Project.States.Cast<AssetDefinition>());

      BoltEditorGUI.HeaderButton("Structs", "mc_struct", () => NewAsset(new StructDefinition()));
      DisplayAssetList(Project.Structs.Cast<AssetDefinition>());

      BoltEditorGUI.HeaderButton("Commands", "mc_command", () => NewAsset(new CommandDefinition()));
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

  void OverlayIcon(string icon) {
    Rect r = GUILayoutUtility.GetLastRect();
    r.xMin = r.xMax - 15;
    r.xMax = r.xMax - 3;
    r.yMin = r.yMin + 2;
    r.yMax = r.yMax - 1;

    GUI.color = BoltRuntimeSettings.instance.highlightColor;
    GUI.DrawTexture(r, BoltEditorGUI.LoadIcon(icon));
    GUI.color = Color.white;
  }

  void DisplayAssetList(IEnumerable<AssetDefinition> assets) {
    bool deleteMode = (Event.current.modifiers & EventModifiers.Control) == EventModifiers.Control;

    foreach (var a in assets.OrderBy(x => x.Name)) {

      // check
      if (HasGroupSelected && !a.Groups.Contains(Project.ActiveGroup)) {
        continue;
      }

      GUILayout.BeginHorizontal();

      GUIStyle style;
      style = new GUIStyle(EditorStyles.miniButtonLeft);
      style.alignment = TextAnchor.MiddleLeft;

      if (IsSelected(a)) {
        style.normal.textColor = BoltRuntimeSettings.instance.highlightColor;
      }

      if (GUILayout.Button(new GUIContent(a.Name), style)) {
        Select(a);
      }

      if (GUILayout.Button(" ", EditorStyles.miniButtonRight, GUILayout.Width(16))) {
        if (deleteMode) {
          a.Deleted = true;

          if (IsSelected(a)) {
            Select(null);
          }

          Save();
        }
        else {
          OpenFilterMenu(a);
        }
      }

      if (deleteMode) {
        OverlayIcon("mc_minus");
      }
      else {
        OverlayIcon("mc_group");
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

  void OpenFilterMenu(AssetDefinition asset) {
    GenericMenu menu = new GenericMenu();

    foreach (string group in Project.Groups) {
      menu.AddItem(new GUIContent(group), asset.Groups.Contains(group), userData => {
        STuple<AssetDefinition, string> pair = (STuple<AssetDefinition, string>)userData;

        if (pair.item0.Groups.Contains(pair.item1)) {
          pair.item0.Groups.Remove(pair.item1);
        }
        else {
          pair.item0.Groups.Add(pair.item1);
        }

        Save();
      }, new STuple<AssetDefinition, string>(asset, group));
    }

    menu.AddItem(new GUIContent(">> New Group"), false, userData => {
      addGroup = "New Group";
      addGroupTo = (AssetDefinition)userData;
    }, asset);

    menu.ShowAsContext();
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
