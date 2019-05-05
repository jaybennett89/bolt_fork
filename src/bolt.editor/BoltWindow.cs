using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEditor;
using Bolt.Compiler;
using System.IO;
using System;
using System.Threading;
using System.Collections.Generic;

[InitializeOnLoad]
static class BoltBackgroundSaver {
  static Thread thread;
  static AutoResetEvent saveEvent;
  static Queue<Project> saveQueue;

  static BoltBackgroundSaver() {
    saveEvent = new AutoResetEvent(false);
    saveQueue = new Queue<Project>();

    thread = new Thread(SaveThread);
    thread.IsBackground = true;
    thread.Start();
  }

  static void SaveThread() {
    while (true) {
      if (saveEvent.WaitOne()) {
        Project project = null;

        lock (saveQueue) {
          while (saveQueue.Count > 0) {
            project = saveQueue.Dequeue();
          }
        }

        if (project != null) {
          try {
            Directory.CreateDirectory(Path.GetDirectoryName(BoltWindow.ProjectTempOldPath));
            Directory.CreateDirectory(Path.GetDirectoryName(BoltWindow.ProjectTempNewPath));

            // copy current project
            if (File.Exists(BoltWindow.ProjectPath)) {
              File.Copy(BoltWindow.ProjectPath, BoltWindow.ProjectTempOldPath, true);
            }

            // write new project
            File.WriteAllBytes(BoltWindow.ProjectTempNewPath, project.ToByteArray());

            // copy new project to correct path
            File.Copy(BoltWindow.ProjectTempNewPath, BoltWindow.ProjectPath, true);
          }
          catch (Exception exn) {
            Debug.LogException(exn);
          }
        }
      }
    }
  }

  static public void Save(Project project) {
    if ((thread == null) || (thread.IsAlive == false)) {
      Debug.LogError("BOLT SAVE THREAD NOT RUNNING");
      return;
    }

    lock (saveQueue) {
      saveQueue.Enqueue(project.DeepClone());
      saveEvent.Set();
    }
  }
}

public abstract class BoltWindow : EditorWindow {
  public static string ProjectPath {
    get { return "Assets/bolt/project.bytes"; }
  }

  public static string ProjectTempNewPath {
    get { return "Temp/bolt/project_new.bytes"; }
  }

  public static string ProjectTempOldPath {
    get { return "Temp/bolt/project_old.bytes"; }
  }

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
    if (HasProject) {
      BoltBackgroundSaver.Save(Project);
    }
  }

  protected void Update() {
    if ((Repaints > 0) || ((repaintTime + 0.05f) < Time.realtimeSinceStartup)) {
      Repaint();
      repaintTime = Time.realtimeSinceStartup;
    }
  }

  protected void OnGUI() {
    if (Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown) {
      const EventModifiers MODS = EventModifiers.Control;

      if ((Event.current.modifiers & MODS) == MODS) {
        Event.current.Use();

        // compile!
        BoltUserAssemblyCompiler.Run();
      }
    }

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
