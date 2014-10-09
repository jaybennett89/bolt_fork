using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoltCommandAsset))]
public class BoltCommandAssetEditor : Editor {

  public override bool UseDefaultMargins () {
    return false;
  }

  public override void OnInspectorGUI() {
    GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
    style.normal.textColor = Color.red;
    GUILayout.Label("Use the new 'Bolt Project' window to add/edit assets", style);

    EditorGUI.BeginDisabledGroup(true);
    BoltCommandAsset asset = (BoltCommandAsset) target;

    BoltAssetEditorGUI.HeaderPropertyList("controller", "Input", ref asset.inputProperties);
    asset.inputProperties = BoltAssetEditorGUI.EditPropertyArray(asset.inputProperties, BoltAssetPropertyEditMode.Command, false);

    BoltAssetEditorGUI.HeaderPropertyList("result", "Result", ref asset.stateProperties);
    asset.stateProperties = BoltAssetEditorGUI.EditPropertyArray(asset.stateProperties, BoltAssetPropertyEditMode.Command, false);

    BoltAssetEditorGUI.CompileButton(asset);
    EditorGUI.EndDisabledGroup();
  }
}
