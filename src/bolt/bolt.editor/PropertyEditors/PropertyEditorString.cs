using UnityEngine;
using System.Collections;
using Bolt.Compiler;
using UnityEditor;

public class PropertyEditorString : PropertyEditor<PropertyTypeString> {
  protected override void Edit(bool array) {
    BoltEditorGUI.WithLabel("Max Length (1 - 100)", () => {
      PropertyType.MaxLength = Mathf.Clamp(EditorGUILayout.IntField(PropertyType.MaxLength), 1, 100);
    });

    BoltEditorGUI.WithLabel("Encoding", () => {
      PropertyType.Encoding = (StringEncodings)EditorGUILayout.EnumPopup(PropertyType.Encoding);
    });
  }
}
