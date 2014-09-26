using Bolt.Compiler;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class BoltProjectWindow : BoltWindow {
  [MenuItem("Window/Bolt Project")]
  public static void Open() {
    BoltProjectWindow w;

    w = EditorWindow.GetWindow<BoltProjectWindow>();
    w.title = "Bolt Project";
    w.name = "Bolt Project";
    w.minSize = new Vector2(150, 200);
    w.Show();
  }

  bool menu;
  bool edit;

  string editName;

  Vector2 scroll;

  AssetFolder ParentForNewAsset {
    get {
      AssetFolder parent = Project.RootFolder;

      if (Selected is AssetFolder) {
        parent = (AssetFolder)Selected;
      }

      // expand parent
      parent.Expanded = true;

      return parent;
    }
  }

  void NewFolder() {
    AssetFolder folder;
    folder = new AssetFolder();
    folder.Guid = Guid.NewGuid();
    folder.Name = "NewFolder";

    // add to parent
    ArrayUtility.Add(ref ParentForNewAsset.Folders, folder);

    // select it
    Select(folder);

    // begin editing name
    BeginEditName();
  }

  void NewState() {
    StateDefinition def;
    def = new StateDefinition();
    def.Guid = Guid.NewGuid();
    def.Name = "NewState";

    // add to parent
    ArrayUtility.Add(ref ParentForNewAsset.Assets, def);

    // select it
    Select(def);

    // begin editing name
    BeginEditName();
  }


  void NewStruct() {
    StructDefinition def;
    def = new StructDefinition();
    def.Guid = Guid.NewGuid();
    def.Name = "NewStruct";

    // add to parent
    ArrayUtility.Add(ref ParentForNewAsset.Assets, def);

    // select it
    Select(def);

    // begin editing name
    BeginEditName();
  }

  void ContextMenu() {
    if (Event.current.type != EventType.Repaint) {
      return;
    }

    if (Repaints == 0) {
      if (menu) {
        menu = false;

        GenericMenu m = new GenericMenu();
        m.AddItem(new GUIContent("New Folder"), false, NewFolder);
        m.AddItem(new GUIContent("New State"), false, NewState);
        m.AddItem(new GUIContent("New Struct"), false, NewStruct);
        m.ShowAsContext();
      }
    }
  }

  void OnGUI() {
    base.OnGUI();

    if (Selected != null) {
      if (Event.current.type == EventType.KeyDown) {
        if (edit) {
          if (Event.current.keyCode == KeyCode.Return) {
            EndEditName(true);
          }
          if (Event.current.keyCode == KeyCode.Escape) {
            EndEditName(false);
          }
        }
        else {
          if (Application.platform == RuntimePlatform.OSXEditor) {
            if (Event.current.keyCode == KeyCode.Return) {
              BeginEditName();
            }
          }

          if (Application.platform == RuntimePlatform.WindowsEditor) {
            if (Event.current.keyCode == KeyCode.F2) {
              BeginEditName();
            }
          }
        }
      }
    }

    if (HasProject) {
      ContextMenu();
    }

    scroll = GUILayout.BeginScrollView(scroll, false, false);

    if (HasProject) {
      Sidebar();
    }

    GUILayout.EndScrollView();

    ShowMenu(null);

    if (GUI.changed) {
      Save();
    }

    ArrowKeys();
  }

  void BeginEditName() {
    editName = "";

    if (edit) {
      EndEditName(false);
    }

    editName = Selected.GetName();
    edit = true;
  }

  void EndEditName(bool save) {
    edit = false;

    if (save) {
      if (Selected is AssetFolder) {
        ((AssetFolder)Selected).Name = editName;
      }

      if (Selected is AssetDefinition) {
        ((AssetDefinition)Selected).Name = editName;
      }
    }

    BeginClearFocus();
    Save();
  }

  void Sidebar() {
    GUILayout.Space(3);
    Folder(Project.RootFolder, 0);
  }

  bool IsSelected(object obj) {
    return ReferenceEquals(obj, Selected);
  }

  void ArrowKeys() {
    if (Selected != null) {
      if (Event.current.isKey) {
        AssetFolder folder = Selected as AssetFolder;

        if (BoltEditorGUI.WasKeyPressed(KeyCode.UpArrow)) {
          // try previous sibling
          if (!SelectPrevSibling()) {
            // if fail, go to parent
            SelectParentFolder();
          }
        }

        if (BoltEditorGUI.WasKeyPressed(KeyCode.DownArrow)) {
          if (folder != null && folder.Expanded) {
            if (!SelectFirstChild()) {
              SelectNextSibling();
            }
          }
          else {
            SelectNextSibling();
          }
        }

        if (folder != null) {
          if (folder.Expanded) {
            if (BoltEditorGUI.WasKeyPressed(KeyCode.LeftArrow)) {
              folder.Expanded = false;
            }

            if (BoltEditorGUI.WasKeyPressed(KeyCode.RightArrow)) {
              SelectFirstChild();
            }
          }
          else {
            if (BoltEditorGUI.WasKeyPressed(KeyCode.LeftArrow)) {
              SelectParentFolder();
            }

            if (BoltEditorGUI.WasKeyPressed(KeyCode.RightArrow)) {
              folder.Expanded = true;
            }
          }
        }
        else {
          if (BoltEditorGUI.WasKeyPressed(KeyCode.LeftArrow)) {
            SelectParentFolder();
          }
        }
      }
    }
  }

  bool SelectFirstChild() {
    if (Selected is AssetFolder) {
      AssetFolder folder = (AssetFolder)Selected;

      if (folder.Children.Count() > 0) {
        Select(folder.Children.First());
        return true;
      }
    }

    return false;
  }

  bool SelectPrevSibling() {
    INamedAsset sibling = Project.RootFolder.FindPrevSibling(Selected);

    if (sibling != null) {
      Select(sibling);
      return true;
    }

    return false;
  }

  bool SelectNextSibling() {
    INamedAsset sibling = Project.RootFolder.FindNextSibling(Selected);

    if (sibling != null) {
      Select(sibling);
      return true;
    }

    return false;
  }

  void SelectParentFolder() {
    AssetFolder parent = Project.RootFolder.FindParentFolder(Selected);

    if (ReferenceEquals(parent, Project.RootFolder) == false) {
      Select(parent);
    }
  }

  void Folder(AssetFolder folder, int indent) {
    var isRoot = ReferenceEquals(folder, Project.RootFolder);

    if (IsSelected(folder) && !edit) {
      GUILayout.BeginHorizontal(BoltEditorGUI.SelectedStyle);
    }
    else {
      GUILayout.BeginHorizontal();
    }

    RectOffset r = new RectOffset(3 + (indent * 11), 0, 0, 0);
    BoltEditorGUI.IconClickable(folder.Expanded ? "boltico_arrow_down_8px" : "boltico_arrow_right_8px", r, () => { if (BoltEditorGUI.IsLeftClick) Expand(folder); }, 9);
    BoltEditorGUI.IconClickable(folder.Expanded ? "boltico_folder_open" : "boltico_folder_closed", () => { SelectOrExpand(folder); });

    GUIStyle label = new GUIStyle(GUI.skin.label);
    label.margin = new RectOffset();

    if (IsSelected(folder) && edit && !isRoot) {
      GUI.SetNextControlName(folder.Guid.ToString());
      editName = EditorGUILayout.TextField(editName);
      GUI.FocusControl(folder.Guid.ToString());
    }
    else {
      BoltEditorGUI.LabelClickable(isRoot ? "Project" : folder.Name, label, () => {
        ShowMenu(folder);
        SelectOrExpand(folder);
      });
    }

    GUILayout.EndHorizontal();

    GUILayout.BeginVertical();

    if (folder.Expanded || isRoot) {
      foreach (AssetFolder subFolder in folder.Folders.OrderBy(x => x.Name)) {
        Folder(subFolder, indent + 1);
      }

      foreach (AssetDefinition asset in folder.Assets.OrderBy(x => x.Name)) {
        Asset(asset, indent + 1);
      }
    }

    GUILayout.EndVertical();
  }

  void ShowMenu(AssetFolder folder) {
    if (BoltEditorGUI.IsRightClick) {
      // open menu
      menu = true;

      // repaint twice
      Repaints = 2;

      if (folder != null) {
        Select(folder);
      }
      else {
        BoltEditorGUI.UseEvent();
      }
    }
  }

  void SelectOrExpand(AssetFolder folder) {
    if (BoltEditorGUI.IsLeftClick) {
      switch (BoltEditorGUI.ClickCount) {
        case 1: Select(folder); break;
        case 2: Expand(folder); break;
      }
    }
  }

  void Select(INamedAsset asset) {
    Repaints = 10;
    Selected = asset;
    EndEditName(false);
    BoltEditorGUI.UseEvent();
  }

  void Expand(AssetFolder folder) {
    folder.Expanded = !folder.Expanded;
    BoltEditorGUI.UseEvent();
  }

  void Asset(AssetDefinition asset, int indent) {
    if (IsSelected(asset) && !edit) {
      GUILayout.BeginHorizontal(BoltEditorGUI.SelectedStyle);
    }
    else {
      GUILayout.BeginHorizontal();
    }

    var icon = "";

    if (asset is StateDefinition) { icon = "boltico_replistate2"; }
    if (asset is StructDefinition) { icon = "boltico_object"; }

    RectOffset r = new RectOffset(12 + (indent * 11), 0, 0, 0);
    BoltEditorGUI.IconClickable(icon, r, () => { Select(asset); });

    GUIStyle label = new GUIStyle(GUI.skin.label);
    label.margin = new RectOffset();

    if (IsSelected(asset) && edit) {
      GUI.SetNextControlName(asset.Guid.ToString());
      editName = EditorGUILayout.TextField(editName);
      GUI.FocusControl(asset.Guid.ToString());
    }
    else {
      BoltEditorGUI.LabelClickable(asset.Name, label, () => { Select(asset); });
    }

    //GUILayout.FlexibleSpace();
    //BoltEditorGUI.Icon("new");

    GUILayout.EndHorizontal();
  }
}
