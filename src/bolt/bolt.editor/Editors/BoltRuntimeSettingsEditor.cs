using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoltRuntimeSettings))]
public class BoltRuntimeSettingsEditor : Editor {
  public override void OnInspectorGUI () {
    BoltRuntimeSettings asset = (BoltRuntimeSettings) target;

    try {
      GUILayout.Label("Prefabs", EditorStyles.boldLabel);
      for (int i = 0; i < asset._prefabs.Length; ++i) {
        GUILayout.Label(asset._prefabs[i].name);
      }

      if (GUI.changed) {
        EditorUtility.SetDirty(target);
      }
    } catch (Exception exn) {
      Debug.LogError("Exception thrown while editing BoltRuntimeSettings, resetting asset");
      Debug.LogException(exn);

      asset._prefabs = new GameObject[0];

      EditorUtility.SetDirty(asset);
    }
  }
}
