using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoltEntity))]
public class BoltEntityEditor : Editor {
  static string[] serializerNames;
  static Bolt.UniqueId[] serializerIds;
  static Bolt.ISerializerFactory[] serializerFactories;

  static BoltEntityEditor() {
    serializerFactories =
      typeof(Bolt.ISerializerFactory)
        .FindInterfaceImplementations()
        .Select(x => Activator.CreateInstance(x))
        .Cast<Bolt.ISerializerFactory>()
        .ToArray();

    serializerNames =
      new string[] { "NOT ASSIGNED" }
        .Concat(serializerFactories.Select(x => x.TypeObject.Name))
        .ToArray();

    serializerIds =
      serializerFactories
        .Select(x => x.TypeUniqueId)
        .ToArray();
  }

  public override void OnInspectorGUI() {
    BoltEntity entity = (BoltEntity)target;
    PrefabType prefabType = PrefabUtility.GetPrefabType(entity.gameObject);

    bool canBeEdited =
      (Application.isPlaying == false) &&
      (
        prefabType == PrefabType.Prefab ||
        prefabType == PrefabType.DisconnectedPrefabInstance ||
        prefabType == PrefabType.None
      );

    BoltRuntimeSettings settings = BoltRuntimeSettings.instance;

    GUILayout.Label("Settings", EditorStyles.boldLabel);
    EditorGUILayout.LabelField("Prefab Type", prefabType.ToString());

    if (prefabType == PrefabType.PrefabInstance || prefabType == PrefabType.DisconnectedPrefabInstance) {
      EditorGUILayout.LabelField("Scene Id", entity.sceneGuid.ToString());

      if (entity.sceneGuid == Bolt.UniqueId.None) {
        // create new scene id
        entity.sceneGuid = Bolt.UniqueId.New();

        // save shit (force)
        EditorUtility.SetDirty(this);

        // log it
        Debug.Log(string.Format("Generated scene {0} for {1}", entity.sceneGuid, entity.gameObject.name));
      }
    }

    EditorGUI.BeginDisabledGroup(!canBeEdited);

    // Prefab Id
    switch (prefabType) {
      case PrefabType.Prefab:
      case PrefabType.PrefabInstance:
        EditorGUILayout.LabelField("Prefab Id", entity._prefabId.ToString());

        if (entity._prefabId < 0) {
          EditorGUILayout.HelpBox("Prefab id not set, run the 'Assets/Bolt Engine/Compile Assembly' menu option to correct", MessageType.Error);
        }

        if (prefabType == PrefabType.Prefab) {
          if (Bolt.PrefabDatabase.Contains(entity) == false) {
            EditorGUILayout.HelpBox("Prefab lookup not valid, run the 'Assets/Bolt Engine/Compile Assembly' menu option to correct", MessageType.Error);
          }
        }

        break;

      case PrefabType.None:
      case PrefabType.DisconnectedPrefabInstance:
        entity._prefabId = EditorGUILayout.IntField("Prefab Id", entity._prefabId);

        if (entity._prefabId < 0) {
          EditorGUILayout.HelpBox("Prefab Id not set", MessageType.Error);
        }
        break;
    }


    // Serializer
    int selectedIndex;
    selectedIndex = Math.Max(0, Array.IndexOf(serializerIds, entity.serializerGuid) + 1);
    selectedIndex = EditorGUILayout.Popup("Serializer", selectedIndex, serializerNames);

    if (selectedIndex == 0) {
      entity.serializerGuid = Bolt.UniqueId.None;
      EditorGUILayout.HelpBox("You must assign a serializer to this prefab before using it", MessageType.Error);
    }
    else {
      entity.serializerGuid = serializerIds[selectedIndex - 1];
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
      if (prefabType == PrefabType.Prefab || prefabType == PrefabType.None) {
        Save();
      }
    }
  }

  void Save() {
    if (!Application.isPlaying && GUI.changed) {
      EditorUtility.SetDirty(target);
    }
  }

  void RuntimeInfoGUI(BoltEntity entity) {
    BoltNetworkInternal.DebugDrawer.IsEditor(true);

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

      GUILayout.Label("Serializer Debug Info", EditorStyles.boldLabel);
      entity.Entity.Serializer.DebugInfo();
    }
  }
}


