using UnityEngine;
using System.Collections;
using Bolt.Compiler;
using UnityEditor;

public class PropertyEditorEntity : PropertyEditor<PropertyTypeEntity> {
  protected override void Edit(bool array) {
    //BoltEditorGUI.WithLabel("Is Parent", () => {
    //  PropertyType.IsParent = EditorGUILayout.Toggle(PropertyType.IsParent); 
    //});
  }
}
