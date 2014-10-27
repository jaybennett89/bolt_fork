using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEditor;
using Bolt.Compiler;
using System.IO;
using System;

public abstract class BoltWindow : EditorWindow {
  public static string ProjectPath {
    get { return "Assets/bolt/project.bytes"; }
  }

  bool save;

  float saveTime;
  float repaintTime;

  static internal Project Project;
  static internal DateTime ProjectModifyTime;

  static bool clear;
  static protected int Repaints;
  static protected AssetDefinition Selected;

  protected bool HasProject {
    get { return Project != null; }
  }

  protected void Save() {
    Save(false);
  }

  protected void Save(bool instant) {
    if (instant) {
      SaveToDisk();
    }
    else {
      save = true;
      saveTime = Time.realtimeSinceStartup + 1f;
    }
  }

  protected void Update() {
    if ((Repaints > 0) || ((repaintTime + 0.05f) < Time.realtimeSinceStartup)) {
      Repaint();
      repaintTime = Time.realtimeSinceStartup;
    }

    if (save && (saveTime < Time.realtimeSinceStartup)) {
      SaveToDisk();
    }
  }

  void SaveToDisk() {
    if (HasProject) {
      try {
        File.WriteAllBytes(ProjectPath, Project.ToByteArray());
      }
      finally {
        save = false;
      }
    }
  }

  protected void OnGUI() {
    BoltEditorGUI.Tooltip = "";

    LoadProject();

    if (Event.current.type == EventType.Repaint) {
      Repaints = Mathf.Max(0, Repaints - 1);
    }

    if (Selected != null && Selected.Deleted) {
      Selected = null;
    }
  }

  protected void LoadProject() {
    if (File.Exists(ProjectPath) == false) {
      Debug.Log("Creating project... " + ProjectPath);

      Project = new Project();
      Save();
    }
    else {
      if (Project == null) {
        Debug.Log("Loading project... " + ProjectPath);
        Project = File.ReadAllBytes(ProjectPath).ToObject<Project>();
        ProjectModifyTime = File.GetLastWriteTime(ProjectPath);

        if (Project.Merged == false) {
          Debug.Log("Merged Project... " + ProjectPath);

          Project.Merged = true;
          Project.RootFolder.Assets = Project.RootFolder.AssetsAll.ToArray();

          Save();
        }
      }
      else {

      }
    }
  }

  protected void ClearAllFocus() {
    if (Event.current.type != EventType.Repaint) {
      return;
    }

    if (Repaints == 0) {
      clear = false;
      return;
    }

    if (clear) {
      GUI.SetNextControlName("ClearFocusFix");
      GUI.Button(new Rect(0, 0, 0, 0), "", GUIStyle.none);
      GUI.FocusControl("ClearFocusFix");
    }
  }

  protected void BeginClearFocus() {
    // we are clearing
    clear = true;

    // repaint a few times
    Repaints = 10;

    // this also helps
    GUIUtility.keyboardControl = 0;
  }
}
