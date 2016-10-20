using UnityEngine;
using System.Collections;
using Bolt.Compiler;
using UnityEditor;

public class PropertyEditorFloat : PropertyEditor<PropertyTypeFloat> {
  protected override void Edit(bool array) {
    BoltEditorGUI.EditSmoothingAlgorithm(Asset, Definition, false);

    if (Asset is StateDefinition) {
      if (Definition.StateAssetSettings.SmoothingAlgorithm == SmoothingAlgorithms.Interpolation) {
        BoltEditorGUI.WithLabel("Interpolation Mode", () => {
          PropertyType.IsAngle = BoltEditorGUI.ToggleDropdown("As Angle", "As Float", PropertyType.IsAngle);
        });
      }
    }

    BoltEditorGUI.WithLabel("Compression", () => {
      PropertyType.Compression = BoltEditorGUI.EditFloatCompression(PropertyType.Compression);
    });

  }
}
