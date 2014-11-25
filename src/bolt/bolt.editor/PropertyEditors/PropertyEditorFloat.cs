using UnityEngine;
using System.Collections;
using Bolt.Compiler;
using UnityEditor;

public class PropertyEditorFloat : PropertyEditor<PropertyTypeFloat> {
  protected override void Edit(bool array) {
    BoltEditorGUI.EditSmoothingAlgorithm(Asset, Definition, false);

    BoltEditorGUI.WithLabel("Compression", () => {
      PropertyType.Compression = BoltEditorGUI.EditFloatCompression(PropertyType.Compression);
    });
  }
}
