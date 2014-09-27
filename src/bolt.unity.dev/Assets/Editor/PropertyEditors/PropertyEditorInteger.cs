using UnityEngine;
using System.Collections;
using Bolt.Compiler;
using UnityEditor;

public class PropertyEditorInteger : PropertyEditor<PropertyTypeInteger> {
  protected override void Edit(bool array) {
    BoltEditorGUI.WithLabel("Mode", () => {
      PropertyType.Mode = (IntegerMode)EditorGUILayout.EnumPopup(PropertyType.Mode);
    });

    BoltEditorGUI.WithLabel("Min Value", () => { PropertyType.MinValue = EditorGUILayout.IntField(PropertyType.MinValue); });
    BoltEditorGUI.WithLabel("Max Value", () => { PropertyType.MaxValue = EditorGUILayout.IntField(PropertyType.MaxValue); });

    BoltEditorGUI.WithLabel("Info", () => {
      EditorGUILayout.LabelField("Bits: " + BoltMath.BitsRequired(PropertyType.MaxValue - PropertyType.MinValue));
    });

    PropertyType.MinValue = Mathf.Min(PropertyType.MinValue, PropertyType.MaxValue - 1);
    PropertyType.MaxValue = Mathf.Max(PropertyType.MaxValue, PropertyType.MinValue + 1);

    if (PropertyType.Mode == IntegerMode.Unsigned) {
      PropertyType.MinValue = Mathf.Max(PropertyType.MinValue, 0);
      PropertyType.MaxValue = Mathf.Max(PropertyType.MaxValue, PropertyType.MinValue + 1);
    }
  }
}
