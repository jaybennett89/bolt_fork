using UnityEngine;
using System.Collections;
using Bolt.Compiler;
using UnityEditor;

public class PropertyEditorString : PropertyEditor<PropertyTypeString> {
  protected override void Edit(bool array) {
    BoltEditorGUI.WithLabel("Encoding & Length", () => {
      PropertyType.Encoding = (StringEncodings)EditorGUILayout.EnumPopup(PropertyType.Encoding);
      PropertyType.MaxLength = Mathf.Clamp(BoltEditorGUI.IntFieldOverlay(PropertyType.MaxLength, "Max Length (1 - 140)"), 1, 140);
    });
  }
}
