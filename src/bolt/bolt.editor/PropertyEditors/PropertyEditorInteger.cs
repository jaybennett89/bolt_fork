using UnityEngine;
using System.Collections;
using Bolt.Compiler;
using UnityEditor;

public class PropertyEditorInteger : PropertyEditor<PropertyTypeInteger> {
  protected override void Edit(bool array) {
    //BoltEditorGUI.WithLabel("Min Value", () => { PropertyType.MinValue = EditorGUILayout.IntField(PropertyType.MinValue); });
    //BoltEditorGUI.WithLabel("Max Value", () => { PropertyType.MaxValue = EditorGUILayout.IntField(PropertyType.MaxValue); });

    //BoltEditorGUI.WithLabel("Info", () => {
    //  EditorGUILayout.LabelField("Bits: " + BoltMath.BitsRequired(PropertyType.MaxValue - PropertyType.MinValue));
    //});

    //PropertyType.MinValue = Mathf.Min(PropertyType.MinValue, PropertyType.MaxValue - 1);
    //PropertyType.MaxValue = Mathf.Max(PropertyType.MaxValue, PropertyType.MinValue + 1);
  }
}
