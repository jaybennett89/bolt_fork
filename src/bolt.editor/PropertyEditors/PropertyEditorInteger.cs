﻿using UnityEngine;
using System.Collections;
using Bolt.Compiler;
using UnityEditor;

public class PropertyEditorInteger : PropertyEditor<PropertyTypeInteger> {
  protected override void Edit(bool array) {

    BoltEditorGUI.WithLabel("Compression", () => {
      PropertyType.CompressionEnabled = BoltEditorGUI.Toggle(PropertyType.CompressionEnabled);


      EditorGUI.BeginDisabledGroup(PropertyType.CompressionEnabled == false);

      PropertyType.MinValue = Mathf.Min(BoltEditorGUI.IntFieldOverlay(PropertyType.MinValue, "Min"), PropertyType.MaxValue - 1);
      PropertyType.MaxValue = Mathf.Max(BoltEditorGUI.IntFieldOverlay(PropertyType.MaxValue, "Max"), PropertyType.MinValue + 1);

      GUILayout.Label("Bits: " + PropertyType.BitsRequired, EditorStyles.miniLabel, GUILayout.ExpandWidth(false));

      EditorGUI.EndDisabledGroup();

    });

    //BoltEditorGUI.WithLabel("Min Value", () => { PropertyType.MinValue = EditorGUILayout.IntField(PropertyType.MinValue); });
    //BoltEditorGUI.WithLabel("Max Value", () => { PropertyType.MaxValue = EditorGUILayout.IntField(PropertyType.MaxValue); });

    //BoltEditorGUI.WithLabel("Info", () => {
    //  EditorGUILayout.LabelField("Bits: " + BoltMath.BitsRequired(PropertyType.MaxValue - PropertyType.MinValue));
    //});

    //PropertyType.MinValue = Mathf.Min(PropertyType.MinValue, PropertyType.MaxValue - 1);
    //PropertyType.MaxValue = Mathf.Max(PropertyType.MaxValue, PropertyType.MinValue + 1);
  }
}
