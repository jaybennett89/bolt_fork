using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoltEntity))]
public class BoltEntityEditor : Editor {
  static int[] serializerIds;
  static string[] serializerNames;
  static Bolt.IFactory[] serializerFactories;

  static BoltEntityEditor() {
    serializerFactories =
      typeof(Bolt.IFactory)
        .FindInterfaceImplementations()
        .Select(x => Activator.CreateInstance(x))
        .Cast<Bolt.IFactory>()
        .ToArray();

    serializerNames =
      new string[] { "<Dynamic>" }
        .Concat(serializerFactories.Select(x => x.TypeObject.Name))
        .ToArray();

    serializerIds =
      serializerFactories
        .Select(x => x.TypeId.Value)
        .ToArray();
  }

  public override void OnInspectorGUI() {
    BoltEntity entity = (BoltEntity)target;
    PrefabType prefabType = PrefabUtility.GetPrefabType(entity.gameObject);
    BoltRuntimeSettings settings = BoltRuntimeSettings.instance;

    GUILayout.Label("Settings", EditorStyles.boldLabel);
    EditorGUI.BeginDisabledGroup((prefabType != PrefabType.Prefab) || Application.isPlaying);

    // Prefab Id
    EditorGUI.BeginDisabledGroup(true);
    EditorGUILayout.LabelField("Prefab Id", entity._prefabId.ToString());
    EditorGUI.EndDisabledGroup();

    if (entity._prefabId < 0) {
      EditorGUILayout.HelpBox("Prefab Id not set, run the 'Assets/Compile Bolt Assets' menu option to correct", MessageType.Error);
    }

    if (prefabType == PrefabType.Prefab) {
      if (BoltRuntimeSettings.ContainsPrefab(entity) == false) {
        EditorGUILayout.HelpBox("Prefab lookup not valid, run the 'Assets/Compile Bolt Assets' menu option to correct", MessageType.Error);
      }
    }

    // Serializer
    int selectedIndex;
    selectedIndex = Math.Max(0, Array.IndexOf(serializerIds, entity._defaultSerializerTypeId) + 1);
    selectedIndex = EditorGUILayout.Popup("Serializer", selectedIndex, serializerNames);

    if (selectedIndex == 0) {
      entity._defaultSerializerTypeId = 0;
    }
    else {
      entity._defaultSerializerTypeId = serializerIds[selectedIndex - 1];
    }

    // Update Rate
    entity._updateRate = EditorGUILayout.IntField("Update Rate", entity._updateRate);

    // Bool Settings
    entity._clientPredicted = EditorGUILayout.Toggle("Controller Prediction", entity._clientPredicted);
    entity._allowInstantiateOnClient = EditorGUILayout.Toggle("Client Can Instantiate", entity._allowInstantiateOnClient);
    entity._persistThroughSceneLoads = EditorGUILayout.Toggle("Dont Destroy On Load", entity._persistThroughSceneLoads);

    EditorGUI.EndDisabledGroup();

    if (Application.isPlaying) {
      if (prefabType != PrefabType.Prefab) {
        RuntimeInfoGUI(entity);
      }
    }
    else {
      if (prefabType == PrefabType.Prefab) {
        if (GUI.changed) {
          EditorUtility.SetDirty(entity);
        }
      }
    }
  }

  void RuntimeInfoGUI(BoltEntity entity) {
    GUILayout.Label("Runtime Info", EditorStyles.boldLabel);
    EditorGUILayout.Toggle("Is Attached", entity.isAttached);

    if (entity.isAttached) {
      EditorGUILayout.Toggle("Is Owner", entity.isOwner);

      if (entity.source != null) {
        EditorGUILayout.LabelField("Source", entity.source.remoteEndPoint.ToString());
      }
      else {
        EditorGUILayout.LabelField("Source", "Local");
      }

      if (entity.controller != null) {
        EditorGUILayout.LabelField("Controller", entity.controller.remoteEndPoint.ToString());
      }
      else {
        EditorGUILayout.LabelField("Controller", entity.hasControl ? "Local" : "None");
      }

      EditorGUILayout.LabelField("Proxy Count", entity.Entity.Proxies.count.ToString());
      EditorGUILayout.LabelField("Serializer", entity.Entity.Serializer.GetType().Name);

      GUILayout.Label("Serializer Debug Info", EditorStyles.boldLabel);
      entity.Entity.Serializer.DebugInfo();
    }
  }
}


