using System;
using System.Collections;
using UE = UnityEngine;

static class BoltGUI {
  public static void LabelText(string label, string text) {
    UE.GUILayout.BeginHorizontal();
    UE.GUILayout.Label(label);
    UE.GUILayout.Label(text);
    UE.GUILayout.EndHorizontal();
  }
}
