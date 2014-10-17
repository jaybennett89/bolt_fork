using UnityEngine;
using System.Collections;
using Bolt.Compiler;
using UnityEditor;

public class PropertyEditorFloat : PropertyEditor<PropertyTypeFloat> {
  protected override void Edit(bool array) {
    BoltEditorGUI.EditSmoothingAlgorithm(Asset, Definition);

    BoltEditorGUI.WithLabel("Compression", () => {
      PropertyType.Compression = BoltEditorGUI.EditFloatCompression(PropertyType.Compression);
    });
  }
}
