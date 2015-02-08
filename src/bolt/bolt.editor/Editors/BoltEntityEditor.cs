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
        .Select(x => x.TypeKey)
        .ToArray();
  }

  public override void OnInspectorGUI() {
    GUILayout.Space(4);

    BoltEditorGUI.Help("Entity Settings", "http://wiki.boltengine.com/wiki/38");

    GUILayout.Space(4);

    GUILayout.BeginHorizontal();
    GUILayout.Space(2);
    GUI.DrawTexture(GUILayoutUtility.GetRect(128, 128, 64, 64, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false)), Resources.Load("BoltLogo") as Texture2D);
    GUILayout.EndHorizontal();

    GUILayout.Space(2);

    EditorGUI.BeginDisabledGroup(Application.isPlaying);

    BoltEntity entity = (BoltEntity)target;
    PrefabType prefabType = PrefabUtility.GetPrefabType(entity.gameObject);

    BoltRuntimeSettings settings = BoltRuntimeSettings.instance;

    EditorGUILayout.LabelField("Prefab Type", prefabType.ToString());

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
        if (entity._prefabId != 0) {
          // force 0 prefab id
          entity._prefabId = 0;

          // set dirty
          EditorUtility.SetDirty(this);
        }

        BoltEditorGUI.Disabled(() => {
          EditorGUILayout.IntField("Prefab Id", entity._prefabId);
        });

        break;

      case PrefabType.DisconnectedPrefabInstance:
        entity._prefabId = EditorGUILayout.IntField("Prefab Id", entity._prefabId);

        if (entity._prefabId < 0) {
          EditorGUILayout.HelpBox("Prefab Id not set", MessageType.Error);
        }
        break;
    }

    EditSerializer(entity);
    EditProperties(entity);
    EditSceneProperties(entity, prefabType);

    EditorGUI.EndDisabledGroup();

    if (prefabType == PrefabType.Prefab) {
      SaveEntity(entity);
    }
    else {
      if (Application.isPlaying) {
        RuntimeInfoGUI(entity);
      }
      else {
        SaveEntity(entity);
      }
    }
  }

  void EditSerializer(BoltEntity entity) {
    int selectedIndex;

    selectedIndex = Math.Max(0, Array.IndexOf(serializerIds, entity.serializerGuid) + 1);
    selectedIndex = EditorGUILayout.Popup("State", selectedIndex, serializerNames);

    if (selectedIndex == 0) {
      entity.serializerGuid = Bolt.UniqueId.None;
      EditorGUILayout.HelpBox("You must assign a serializer to this prefab before using it", MessageType.Error);
    }
    else {
      entity.serializerGuid = serializerIds[selectedIndex - 1];
    }
  }


  void EditProperties(BoltEntity entity) {
    BoltRuntimeSettings settings = BoltRuntimeSettings.instance;

    // Update Rate
    entity._updateRate = EditorGUILayout.IntField("Update Rate", entity._updateRate);
    entity._autoFreezeProxyFrames = EditorGUILayout.IntField("Auto Freeze Frames", entity._autoFreezeProxyFrames);

    // Bool Settings
    entity._clientPredicted = EditorGUILayout.Toggle("Controller Prediction", entity._clientPredicted);
    entity._persistThroughSceneLoads = EditorGUILayout.Toggle("Persist Through Load", entity._persistThroughSceneLoads);
    entity._alwaysProxy = EditorGUILayout.Toggle("Always Proxy", entity._alwaysProxy);

    if (settings.clientCanInstantiateAll == false) {
      entity._allowInstantiateOnClient = EditorGUILayout.Toggle("Client Can Instantiate", entity._allowInstantiateOnClient);
    }
  }

  void EditSceneProperties(BoltEntity entity, PrefabType prefabType) {
    bool isSceneObject = prefabType == PrefabType.PrefabInstance || prefabType == PrefabType.DisconnectedPrefabInstance || prefabType == PrefabType.None;

    GUILayout.Label("Scene Object Settings", EditorStyles.boldLabel);

    entity._sceneObjectAutoAttach = EditorGUILayout.Toggle("Attach On Load", entity._sceneObjectAutoAttach);
    entity._sceneObjectDestroyOnDetach = EditorGUILayout.Toggle("Destroy On Detach", entity._sceneObjectDestroyOnDetach);

    if (isSceneObject) {
      if (!Application.isPlaying && (entity.sceneGuid == Bolt.UniqueId.None)) {
        // create new scene id
        entity.sceneGuid = Bolt.UniqueId.New();

        // save shit (force)
        EditorUtility.SetDirty(this);

        // log it
        Debug.Log(string.Format("Generated scene {0} for {1}", entity.sceneGuid, entity.gameObject.name));
      }

      EditorGUILayout.LabelField("Scene Id", entity.sceneGuid.ToString());
    }
  }

  void SaveEntity(BoltEntity entity) {
    if (GUI.changed) {
      EditorUtility.SetDirty(entity);
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


