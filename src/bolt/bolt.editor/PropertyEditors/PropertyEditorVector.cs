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

    var cmdSettings = Definition.CommandAssetSettings;
    var stateSettings = Definition.StateAssetSettings;

    BoltEditorGUI.WithLabel("Snap Magnitude", () => {
      if (cmdSettings != null) {
        cmdSettings.SnapMagnitude = EditorGUILayout.FloatField(cmdSettings.SnapMagnitude);
      }

      if (stateSettings != null) {
        stateSettings.SnapMagnitude = EditorGUILayout.FloatField(stateSettings.SnapMagnitude);
      }
    });

    BoltEditorGUI.WithLabel("Axis Compression", () => {
      BoltEditorGUI.EditAxes(PropertyType.Compression, PropertyType.Selection);
    });

  }
}
