using UnityEngine;
using System.Collections;
using Bolt.Compiler;
using UnityEditor;

public class PropertyEditorTransform : PropertyEditor<PropertyTypeTransform> {

  public enum AxisSelections {
    XYZ,
    XY,
    XZ,
    YZ,
    X,
    Y,
    Z,
  }

  protected override void Edit(bool array) {
    BoltEditorGUI.WithLabel("Algorithm", () => {
      Definition.StateAssetSettings.SmoothingAlgorithm = 
        (SmoothingAlgorithms)EditorGUILayout.EnumPopup(Definition.StateAssetSettings.SmoothingAlgorithm);
    });

    //BoltEditorGUI.WithLabel("Rotation Mode", () => { PropertyType.RotationMode = (TransformRotationMode)EditorGUILayout.EnumPopup(PropertyType.RotationMode); });

    //BoltEditorGUI.SettingsSectionDouble("Position Axes", "Rotation Axes", () => {
    //  EditorGUILayout.BeginHorizontal();
    //  EditorGUILayout.EnumPopup(AxisSelections.XYZ);
    //  EditorGUILayout.EnumPopup(AxisSelections.XYZ);
    //  EditorGUILayout.EndHorizontal();
    //});

    //bool test = true;

    //BoltEditorGUI.SettingsSectionToggle("Compression", ref test, () => {

    //}, GUILayout.Width(70));


    //EditorGUILayout.BeginHorizontal();
    //BoltEditorGUI.EditFloatCompression(PropertyType.GetPositionAxis(VectorComponents.X).Compression, true);
    //GUILayout.Space(5);
    //BoltEditorGUI.EditFloatCompression(PropertyType.GetPositionAxis(VectorComponents.Y).Compression, true);
    //GUILayout.Space(5);
    //BoltEditorGUI.EditFloatCompression(PropertyType.GetPositionAxis(VectorComponents.Z).Compression, true);
    //EditorGUILayout.EndHorizontal();
    //string subType =
    //  PropertyType.GetPositionAxis(VectorComponents.Z).Enabled
    //    ? "Vector3"
    //    : "Vector2";


    //&BoltEditorGUI.EditAxes("Rotation", PropertyType.RotationAxes);

    //PropertyType.RotationCompressionQuaternion = BoltEditorGUI.EditFloatCompression("Rotation", PropertyType.RotationCompressionQuaternion);
  }
}
