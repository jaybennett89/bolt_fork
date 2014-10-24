using UnityEngine;
using System.Collections;
using Bolt.Compiler;
using UnityEditor;
using System;
using System.Linq;

public class PropertyEditorTransform : PropertyEditor<PropertyTypeTransform> {

  protected override void Edit(bool array) {
    BoltEditorGUI.EditSmoothingAlgorithm(Asset, Definition);

    BoltEditorGUI.WithLabel("Axes", () => {
      PropertyType.PositionSelection = BoltEditorGUI.EditAxisSelection("Position: ", PropertyType.PositionSelection);
      PropertyType.RotationSelection = BoltEditorGUI.EditAxisSelection("Rotation: ", PropertyType.RotationSelection);
    });

    if (PropertyType.PositionSelection != AxisSelections.Disabled) {
      BoltEditorGUI.WithLabel("Snap Magnitude", () => {
        Definition.StateAssetSettings.SnapMagnitude = EditorGUILayout.FloatField(Definition.StateAssetSettings.SnapMagnitude);
      });

      BoltEditorGUI.WithLabel("Position Axis Compression", () => {
        BoltEditorGUI.EditAxes(PropertyType.PositionCompression, PropertyType.PositionSelection);
      });
    }

    if (PropertyType.RotationSelection != AxisSelections.Disabled) {
      var quaternion = PropertyType.RotationSelection == AxisSelections.XYZ;

      BoltEditorGUI.WithLabel(quaternion ? "Quaternion Compression" : "Rotation Axis Compression", () => {
        if (quaternion) {
          PropertyType.RotationCompressionQuaternion = BoltEditorGUI.EditFloatCompression(PropertyType.RotationCompressionQuaternion);
        }
        else {
          BoltEditorGUI.EditAxes(PropertyType.RotationCompression, PropertyType.RotationSelection);
        }
      });
    }
  }

}
