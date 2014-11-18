using Bolt.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

class PropertyEditorQuaternion : PropertyEditor<PropertyTypeQuaternion> {
  protected override void Edit(bool array) {
    BoltEditorGUI.EditSmoothingAlgorithm(Asset, Definition, false);

    BoltEditorGUI.WithLabel("Axes", () => {
      PropertyType.Selection = BoltEditorGUI.EditAxisSelection(PropertyType.Selection);
    });

    if (PropertyType.Selection != AxisSelections.Disabled) {
      var quaternion = PropertyType.Selection == AxisSelections.XYZ;

      BoltEditorGUI.WithLabel(quaternion ? "Quaternion Compression" : "Axis Compression", () => {
        if (quaternion) {
          PropertyType.QuaternionCompression = BoltEditorGUI.EditFloatCompression(PropertyType.QuaternionCompression);
        }
        else {
          BoltEditorGUI.EditAxes(PropertyType.EulerCompression, PropertyType.Selection);
        }
      });
    }
  }
}
