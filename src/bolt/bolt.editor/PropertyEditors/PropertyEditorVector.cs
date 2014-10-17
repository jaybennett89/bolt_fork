using Bolt.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

class PropertyEditorVector : PropertyEditor<PropertyTypeVector> {
  protected override void Edit(bool array) {
    BoltEditorGUI.EditSmoothingAlgorithm(Asset, Definition);

    BoltEditorGUI.WithLabel("Axes", () => {
      PropertyType.Selection = BoltEditorGUI.EditAxisSelection(PropertyType.Selection);
    });

    if (PropertyType.Selection != AxisSelections.Disabled) {
      BoltEditorGUI.WithLabel("Axis Compression", () => {
        BoltEditorGUI.EditAxes(PropertyType.Compression, PropertyType.Selection);
      });
    }
  }
}
