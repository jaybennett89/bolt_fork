using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoltEventAsset))]
public class BoltEventAssetEditor : Editor {
  public override void OnInspectorGUI () {
    BoltEventAsset asset = (BoltEventAsset) target;

    // event specific stuff editor
    GUILayout.Label("Settings", EditorStyles.boldLabel);

    GUI.color = BoltAssetEditorGUI.lightOrange;
    BoltAssetEditorGUI.EditBox(BoltAssetEditorGUI.BoxStyle(4), () => {
      GUI.color = Color.white;
      EditorGUILayout.BeginVertical();

      BoltEventDeliveryMode deliveryMode = asset.deliveryMode;


      BoltAssetEditorGUI.DarkLabel("Target", () => {
        asset.eventMode = (BoltAssetEventMode) EditorGUILayout.EnumPopup(asset.eventMode);
      });


      BoltAssetEditorGUI.DarkLabel("Delivery", () => {
        asset.deliveryMode = (BoltEventDeliveryMode) EditorGUILayout.EnumPopup(asset.deliveryMode);
      });

      if (asset.eventMode == BoltAssetEventMode.Entity && asset.deliveryMode == BoltEventDeliveryMode.Reliable) {
        Debug.LogError("Only global events can be reliable");

        if (deliveryMode != BoltEventDeliveryMode.Reliable) {
          asset.deliveryMode = deliveryMode;
        } else {
          asset.deliveryMode = BoltEventDeliveryMode.Unreliable;
        }
      }

      switch (asset.eventMode) {
        case BoltAssetEventMode.Entity:
          GUILayout.BeginHorizontal();
          BoltAssetEditorGUI.DarkLabel("Senders", () => {
            asset.entitySource = BoltAssetEditorGUI.ToggleRow<BoltAssetEventEntitySource>(asset.entitySource);
          });
          GUILayout.EndHorizontal();

          GUILayout.BeginHorizontal();
          BoltAssetEditorGUI.DarkLabel("Receivers", () => {
            asset.entityTarget = BoltAssetEditorGUI.ToggleRow<BoltAssetEventEntityTarget>(asset.entityTarget);
          });
          GUILayout.EndHorizontal();
          break;

        case BoltAssetEventMode.Global:
          GUILayout.BeginHorizontal();
          BoltAssetEditorGUI.DarkLabel("Senders", () => {
            asset.globalSource = BoltAssetEditorGUI.ToggleRow<BoltAssetEventGlobalSource>(asset.globalSource);
          });
          GUILayout.EndHorizontal();

          GUILayout.BeginHorizontal();
          BoltAssetEditorGUI.DarkLabel("Receivers", () => {
            asset.globalTarget = BoltAssetEditorGUI.ToggleRow<BoltAssetEventGlobalTarget>(asset.globalTarget);
          });
          GUILayout.EndHorizontal();
          break;
      }

      EditorGUILayout.EndVertical();
    });

    // property editor
    GUILayout.Label("Properties", EditorStyles.boldLabel);
    asset.properties = BoltAssetEditorGUI.EditPropertyArray(asset.properties, BoltAssetPropertyEditMode.Event, false);

    // compile button
    BoltAssetEditorGUI.CompileButton(asset);
  }
}