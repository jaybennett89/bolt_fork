using UnityEngine;
using System.Collections;
using UnityEditor;
using Bolt.Compiler;
using System.IO;

public abstract class BoltWindow : EditorWindow {
  public static string ProjectPath {
    get {
      var s = BoltRuntimeSettings.instance;

      if (s) {
        var p = s.projectPath.Trim();

        if (string.IsNullOrEmpty(p) == false) {
          return p;
        }
      }

      return "Assets/bolt/project.bytes";
    }
  }

  bool save;
  bool clear;

  float saveTime;
  float repaintTime;

  static protected int Repaints;
  static protected Project Project;
  static protected INamedAsset Selected;
  static protected AssetDefinition SelectedAsset;

  protected bool HasProject {
    get { return Project != null; }
  }

  protected void Save() {
    save = true;
    saveTime = Time.realtimeSinceStartup + 2f;
  }

  protected void Update() {
    if ((Repaints > 0) || ((repaintTime + 0.05f) < Time.realtimeSinceStartup)) {
      Repaint();
      repaintTime = Time.realtimeSinceStartup;
    }

    if (save && (saveTime < Time.realtimeSinceStartup) && HasProject) {
      try {
        File.WriteAllBytes(ProjectPath, Project.ToByteArray());
      }
      finally {
        save = false;
      }
    }
  }

  protected void OnGUI() {
    LoadProject();

    if (Event.current.type == EventType.Repaint) {
      Repaints = Mathf.Max(0, Repaints - 1);
    }

    if (SelectedAsset != null && SelectedAsset.Deleted) {
      SelectedAsset = null;
    }
  }

  protected void LoadProject() {
    if (Project == null) {
      Debug.Log("Loading project ... " + ProjectPath);

      if (File.Exists(ProjectPath) == false) {
        InitNewProject();
      }

      Project = File.ReadAllBytes(ProjectPath).ToObject<Project>();
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
      GUI.Button(new Rect(0, 0, 0, 0), "");
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

  void InitNewProject() {
    File.WriteAllBytes(ProjectPath, new Project().ToByteArray());
  }
}
