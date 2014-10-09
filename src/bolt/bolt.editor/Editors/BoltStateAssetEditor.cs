using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoltStateAsset))]
public class BoltStateAssetEditor : Editor {

  public override bool UseDefaultMargins () {
    return false;
  }

  public override void OnInspectorGUI() {
    GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
    style.normal.textColor = Color.red;
    GUILayout.Label("Use the new 'Bolt Project' window to add/edit assets", style);

    EditorGUI.BeginDisabledGroup(true);
    BoltStateAsset asset = (BoltStateAsset) target;
    
    BoltAssetEditorGUI.HeaderPropertyList("properties", "Properties", ref asset._properties);

    asset.tranform = BoltAssetEditorGUI.EditProperty(asset.tranform, BoltAssetPropertyEditMode.State, false);
    asset.mecanim = BoltAssetEditorGUI.EditProperty(asset.mecanim, BoltAssetPropertyEditMode.State, true);

    asset._properties = BoltAssetEditorGUI.EditPropertyArray(asset._properties, BoltAssetPropertyEditMode.State, false);

    //asset._groups = EditGroupArray(asset._groups);

    BoltAssetEditorGUI.CompileButton(asset);
    EditorGUI.EndDisabledGroup();
  }

  BoltAssetPropertyGroup[] EditGroupArray (BoltAssetPropertyGroup[] ga) {
    for (int i = 0; i < ga.Length; ++i) {
      ga[i] = EditGroup(ga[i]);
    }

    for (int i = 0; i < ga.Length; ++i) {
      if (ga[i] == null) { ArrayUtility.RemoveAt(ref ga, i); }
    }

    //if (GUILayout.Button("New Group", EditorStyles.miniButton)) {
    //ArrayUtility.Add(ref ga, new BoltAssetPropertyGroup());
    //}

    return ga;

  }

  BoltAssetPropertyGroup EditGroup (BoltAssetPropertyGroup g) {
    BoltAssetPropertyGroup result = g;

    GUI.color = BoltAssetEditorGUI.lightOrange;
    BoltAssetEditorGUI.EditBox(BoltAssetEditorGUI.BoxStyle(4), () => {
      GUI.color = Color.white;

      g.enabled = GUILayout.Toggle(g.enabled, "", GUILayout.Width(14));
      g.name = GUILayout.TextField(g.name);

      EditorGUI.BeginDisabledGroup(true);
      EditorGUILayout.EnumPopup(BoltAssetSyncMode.Changed, GUILayout.Width(65));
      EditorGUI.EndDisabledGroup();

      g.syncTarget = BoltAssetEditorGUI.ToggleRow<BoltAssetSyncTarget>((int) g.syncTarget);

      if (BoltAssetEditorGUI.DeleteButton()) {
        result = null;
      }
    });

    EditorGUI.BeginDisabledGroup(g.enabled == false);
    g._properties = BoltAssetEditorGUI.EditPropertyArray(g._properties, BoltAssetPropertyEditMode.State, true);
    EditorGUI.EndDisabledGroup();

    return result;
  }

}