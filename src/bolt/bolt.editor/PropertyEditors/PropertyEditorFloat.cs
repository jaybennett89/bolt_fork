using UnityEngine;
using System.Collections;
using Bolt.Compiler;

public class PropertyEditorFloat : PropertyEditor<PropertyTypeFloat> {
  protected override void Edit(bool array) {
    BoltEditorGUI.SettingsSectionToggle("Compression", ref PropertyType.Compression.Enabled, () => {
      PropertyType.Compression = BoltEditorGUI.EditFloatCompression(PropertyType.Compression, false);
    }, GUILayout.Width(70));
  }
}
