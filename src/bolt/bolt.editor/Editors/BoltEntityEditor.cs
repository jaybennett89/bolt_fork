﻿using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoltEntity))]
public class BoltEntityEditor : Editor {
  public override void OnInspectorGUI () {
    BoltEntity entity = (BoltEntity) target;
    PrefabType prefabType = PrefabUtility.GetPrefabType(entity.gameObject);

    if (prefabType == PrefabType.PrefabInstance) {
      if (entity.boltIsSceneObject == false) {
        entity.boltIsSceneObject = true;
        EditorUtility.SetDirty(entity);
      }
    } else {
      if (entity.boltIsSceneObject) {
        entity.boltIsSceneObject = false;
        EditorUtility.SetDirty(entity);
      }
    }

    if (prefabType == PrefabType.Prefab || prefabType == PrefabType.PrefabInstance) {
      // Scene object
      EditorGUI.BeginDisabledGroup(true);
      EditorGUILayout.Toggle("Scene Object", entity.boltIsSceneObject);
      EditorGUI.EndDisabledGroup();

      // prefab id
      EditorGUI.BeginDisabledGroup(prefabType == PrefabType.PrefabInstance);
      EditorGUI.BeginDisabledGroup(true);
      EditorGUILayout.LabelField("Prefab Id", entity.boltPrefabId.ToString());
      EditorGUI.EndDisabledGroup();

      if (entity.boltPrefabId < 0) {
        EditorGUILayout.HelpBox("Prefab Id not set, run the Bolt/Compile command to correct", MessageType.Error);
      }

      if (prefabType == PrefabType.Prefab) {
        if (BoltRuntimeSettings.ContainsPrefab(entity) == false) {
          EditorGUILayout.HelpBox("Prefab lookup not valid, run the Bolt/Compile command to correct", MessageType.Error);
        }
      }

      // entity callback
      BoltEntitySerializer aref = entity.GetField<BoltEntitySerializer>("_serializer");
      aref = EditorGUILayout.ObjectField("Serializer", aref, typeof(BoltEntitySerializer), false) as BoltEntitySerializer;
      entity.SetField("_serializer", aref);

      if (!aref) {
        EditorGUILayout.HelpBox("Serializer not attached, drag and drop an entity serializer component to correct", MessageType.Warning);
      }

      // persistance mode
      BoltEntityPersistanceMode pmode = entity.GetField<BoltEntityPersistanceMode>("_persistanceMode");
      pmode = (BoltEntityPersistanceMode) EditorGUILayout.EnumPopup("Persistance Mode", pmode);
      entity.SetField("_persistanceMode", pmode);

      // update rate
      entity.SetField("_updateRate", EditorGUILayout.IntField("Update Rate", entity.GetField<int>("_updateRate")));

      //
      entity._clientPredicted = EditorGUILayout.Toggle("Controller Prediction", entity._clientPredicted);

      // save changes
      if (GUI.changed) {
        EditorUtility.SetDirty(entity);
      }

      EditorGUI.EndDisabledGroup();
    }
  }
}
