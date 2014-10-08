using System;
using System.Collections;
using UE = UnityEngine;

static class BoltGUI {
  public static void Label(object label) {
    UE.GUILayout.Label(label.ToString());
  }

  public static void LabelText(string label, string text) {
    UE.GUILayout.BeginHorizontal();
    UE.GUILayout.Label(label);
    UE.GUILayout.Label(text);
    UE.GUILayout.EndHorizontal();
  }
}
