using UnityEngine;
using System.Collections;
using UnityEditor;
using Bolt.Compiler;
using System.Linq;
using System;
using System.Collections.Generic;

public class BoltFilterWindow : BoltWindow {
  [MenuItem("Window/Bolt Filters")]
  public static void Open() {
    BoltFilterWindow w;

    w = EditorWindow.GetWindow<BoltFilterWindow>();
    w.title = "Bolt Filters";
    w.name = "Bolt Filters";
    w.minSize = new Vector2(200, 400);
    w.Show();
  }

  void OnGUI() {
    base.OnGUI();


  }
}
