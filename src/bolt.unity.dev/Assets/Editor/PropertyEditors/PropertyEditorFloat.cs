using UnityEngine;
using System.Collections;
using Bolt.Compiler;

public class PropertyEditorFloat : PropertyEditor<PropertyTypeFloat> {
  protected override void Edit(bool array) {
    PropertyType.Compression = BoltEditorGUI.EditFloatCompression(PropertyType.Compression);
  }
}
