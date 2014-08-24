using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoltMecanimAsset))]
public class BoltMecanimAssetEditor : Editor {

  public override void OnInspectorGUI () {
    BoltMecanimAsset asset = (BoltMecanimAsset) target;

    GUILayout.Label("Settings", EditorStyles.boldLabel);

    GUI.color = BoltAssetEditorGUI.lightOrange;
    BoltAssetEditorGUI.EditBox(BoltAssetEditorGUI.BoxStyle(4), () => {
      GUI.color = Color.white;

      EditorGUILayout.BeginVertical();

      asset.controller = (RuntimeAnimatorController) EditorGUILayout.ObjectField(asset.controller, typeof(RuntimeAnimatorController), false);

      EditorGUILayout.BeginHorizontal();
      asset.replicateLayerWeights = EditorGUILayout.Toggle(asset.replicateLayerWeights, GUILayout.Width(12));

      GUIStyle s = new GUIStyle(EditorStyles.label);
      s.normal.textColor = new Color(0.125f, 0.125f, 0.125f);
      GUILayout.Label("Replicate Layer Weights", s);
      
      EditorGUILayout.EndHorizontal();
      EditorGUILayout.EndVertical();
    });

    if (asset.controller) {
      BoltEditorUtils.SynchronizeWithController(asset);

      // display editor
      GUILayout.Label("Properties", EditorStyles.boldLabel);
      asset.properties = BoltAssetEditorGUI.EditPropertyArray(asset.properties, BoltAssetPropertyEditMode.Mecanim, false);

      // warn if we have any invalid names
      for (int i = 0; i < asset.properties.Length; ++i) {
        BoltAssetProperty p = asset.properties[i];

        if (p.name != p.name.CSharpIdentifier()) {
          EditorGUILayout.HelpBox(string.Format("Parameter '{0}' does not have a name which reperensts a valid C# identifier", p.name), MessageType.Warning);
        }
      }

    } else {
      EditorGUILayout.HelpBox("Select an animation controller to compile", MessageType.Info);
    }

    BoltAssetEditorGUI.CompileButton(asset);
  }
}
