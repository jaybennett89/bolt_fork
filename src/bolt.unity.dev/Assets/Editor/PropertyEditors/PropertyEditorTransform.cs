using UnityEngine;
using System.Collections;
using Bolt.Compiler;
using UnityEditor;

public class PropertyEditorTransform : PropertyEditor<PropertyTypeTransform> {
  protected override void Edit(bool array) {
    BoltEditorGUI.WithLabel("Transform Space", () => { PropertyType.Space = (TransformSpaces)EditorGUILayout.EnumPopup(PropertyType.Space); });
    BoltEditorGUI.WithLabel("Rotation Mode", () => { PropertyType.RotationMode = (TransformRotationMode)EditorGUILayout.EnumPopup(PropertyType.RotationMode); });

    BoltEditorGUI.EditAxes("Position", PropertyType.PositionAxes);

    switch (PropertyType.RotationMode) {
      case TransformRotationMode.QuaternionComponents:
        PropertyType.RotationCompressionQuaternion = BoltEditorGUI.EditFloatCompression("Rotation", PropertyType.RotationCompressionQuaternion);
        break;

      case TransformRotationMode.EulerAngles:
        BoltEditorGUI.EditAxes("Rotation", PropertyType.RotationAxesEuler);
        break;
    }
  }
}
