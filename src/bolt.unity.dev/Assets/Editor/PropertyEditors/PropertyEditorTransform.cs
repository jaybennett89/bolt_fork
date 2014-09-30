using UnityEngine;
using System.Collections;
using Bolt.Compiler;
using UnityEditor;

public class PropertyEditorTransform : PropertyEditor<PropertyTypeTransform> {
  protected override void Edit(bool array) {
      BoltEditorGUI.WithLabel("Space", () => { PropertyType.Space = (TransformSpaces)EditorGUILayout.EnumPopup(PropertyType.Space); });
  }
}
