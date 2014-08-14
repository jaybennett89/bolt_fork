using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoltCommandAsset))]
public class BoltCommandAssetEditor : Editor {

  public override void OnInspectorGUI () {
    BoltCommandAsset asset = (BoltCommandAsset) target;

    GUILayout.Label("Input", EditorStyles.boldLabel);
    asset.inputProperties = BoltAssetEditorGUI.EditPropertyArray(asset.inputProperties, BoltAssetPropertyEditMode.Command, false);

    GUILayout.Label("State", EditorStyles.boldLabel);
    asset.stateProperties = BoltAssetEditorGUI.EditPropertyArray(asset.stateProperties, BoltAssetPropertyEditMode.Command, false);

    BoltAssetEditorGUI.CompileButton(asset);
  }
}
