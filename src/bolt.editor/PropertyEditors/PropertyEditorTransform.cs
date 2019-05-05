using UnityEngine;
using System.Collections;
using Bolt.Compiler;
using UnityEditor;
using System;
using System.Linq;

public class PropertyEditorTransform : PropertyEditor<PropertyTypeTransform> {

  protected override void Edit(bool array) {
    BoltEditorGUI.WithLabel("Space", () => {
      PropertyType.Space = (TransformSpaces)EditorGUILayout.EnumPopup(PropertyType.Space);
    });

    BoltEditorGUI.EditSmoothingAlgorithm(Asset, Definition);

    BoltEditorGUI.Header("Position", "mc_position");

    BoltEditorGUI.WithLabel("Axes", () => {
      PropertyType.PositionSelection = BoltEditorGUI.EditAxisSelection(PropertyType.PositionSelection);
    });

    if (PropertyType.PositionSelection != AxisSelections.Disabled) {
      if (Asset is StateDefinition) {
        BoltEditorGUI.WithLabel("Strict Comparison", () => {
          PropertyType.PositionStrictCompare = EditorGUILayout.Toggle(PropertyType.PositionStrictCompare);
        });

        BoltEditorGUI.WithLabel("Teleport Threshold", () => {
          Definition.StateAssetSettings.SnapMagnitude = EditorGUILayout.FloatField(Definition.StateAssetSettings.SnapMagnitude);
        });
      }

      BoltEditorGUI.WithLabel("Compression", () => {
        BoltEditorGUI.EditAxes(PropertyType.PositionCompression, PropertyType.PositionSelection);
      });
    }


    BoltEditorGUI.Header("Rotation", "mc_rotation");

    BoltEditorGUI.WithLabel("Axes", () => {
      PropertyType.RotationSelection = BoltEditorGUI.EditAxisSelection(PropertyType.RotationSelection);
    });

    if (PropertyType.RotationSelection != AxisSelections.Disabled) {
      if (Asset is StateDefinition) {
        BoltEditorGUI.WithLabel("Strict Comparison", () => {
          PropertyType.RotationStrictCompare = EditorGUILayout.Toggle(PropertyType.RotationStrictCompare);
        });
      }

      var quaternion = PropertyType.RotationSelection == AxisSelections.XYZ;

      BoltEditorGUI.WithLabel(quaternion ? "Compression (Quaternion)" : "Compression (Euler)", () => {
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
