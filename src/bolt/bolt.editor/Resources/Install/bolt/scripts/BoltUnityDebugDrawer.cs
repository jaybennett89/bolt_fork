using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BoltInternal {
  public class UnityDebugDrawer : BoltInternal.IDebugDrawer {
    void BoltInternal.IDebugDrawer.Indent(int level) {
#if UNITY_EDITOR
      UnityEditor.EditorGUI.indentLevel = level;
#endif
    }

    void BoltInternal.IDebugDrawer.Label(string text) {
#if UNITY_EDITOR
      GUILayout.Label(text);
#endif
    }

    void BoltInternal.IDebugDrawer.LabelBold(string text) {
#if UNITY_EDITOR
      GUILayout.Label(text, EditorStyles.boldLabel);
#endif
    }

    void BoltInternal.IDebugDrawer.LabelField(string text, object value) {
#if UNITY_EDITOR
      UnityEditor.EditorGUILayout.LabelField(text, value.ToString());
#endif
    }

    void BoltInternal.IDebugDrawer.Separator() {
#if UNITY_EDITOR
      UnityEditor.EditorGUILayout.Separator();
#endif
    }
  }
}