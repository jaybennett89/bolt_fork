using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoltMecanimAsset))]
public class BoltMecanimAssetEditor : Editor {

  public override bool UseDefaultMargins () {
    return false;
  }

  public override void OnInspectorGUI() {
    GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
    style.normal.textColor = Color.red;
    GUILayout.Label("Use the new 'Bolt Project' window to add/edit assets", style);

    EditorGUI.BeginDisabledGroup(true);
    BoltMecanimAsset asset = (BoltMecanimAsset) target;

    BoltEditorGUI.Header("Settings", "settings");

    BoltAssetEditorGUI.Label("Mecanim Controller", () => {
      asset.controller = (RuntimeAnimatorController) EditorGUILayout.ObjectField(asset.controller, typeof(RuntimeAnimatorController), false);
    });

    BoltAssetEditorGUI.Label("Replicate Layer Weights", () => {
      asset.replicateLayerWeights = EditorGUILayout.Toggle(asset.replicateLayerWeights, GUILayout.Width(12));
    });

    if (asset.controller) {
      BoltEditorUtilsInternal.SynchronizeWithController(asset);

      // display editor
      BoltAssetEditorGUI.HeaderPropertyList("properties", "Properties", ref asset.properties);
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

    EditorGUI.EndDisabledGroup();
  }
}
