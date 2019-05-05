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

  void HelpBox(string text) {
    BoltRuntimeSettings settings = BoltRuntimeSettings.instance;

    if (settings.showBoltEntityHints) {
      EditorGUILayout.HelpBox(text, MessageType.Info);
    }
  }

  public override void OnInspectorGUI() {
    BoltRuntimeSettings settings = BoltRuntimeSettings.instance;

    GUIStyle s = new GUIStyle(EditorStyles.boldLabel);
    s.normal.textColor = new Color(0xf2 / 255f, 0xad / 255f, 0f);
    
    BoltEntity entity = (BoltEntity)target;

    GUILayout.BeginHorizontal();
    GUI.DrawTexture(GUILayoutUtility.GetRect(128, 128, 64, 64, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false)), Resources.Load("BoltLogo") as Texture2D);
    GUILayout.EndHorizontal();

    GUILayout.Label("Prefab & State", s);

    DrawPrefabInfo(entity);
    EditState(entity);

    GUILayout.Label("Settings", s);

    entity._updateRate = EditorGUILayout.IntField("Replication Rate", entity._updateRate);
    HelpBox("Controls how often this entity should be considered for replication. 1 = Every packet, 2 = Every other packet, etc.");

    entity._persistThroughSceneLoads = EditorGUILayout.Toggle("Persistent", entity._persistThroughSceneLoads);
    HelpBox("If enabled Bolt will not destroy this object when a new scene is loaded through BoltNetwork.LoadScene().");

    entity._alwaysProxy = EditorGUILayout.Toggle("Always Replicate", entity._alwaysProxy);
    HelpBox("If enabled Bolt will always replicate this entity and its state, even when operations that normally would block replication is taking place (example: loading a scene).");

    entity._allowFirstReplicationWhenFrozen = EditorGUILayout.Toggle("Proxy When Frozen", entity._allowFirstReplicationWhenFrozen);
    HelpBox("If enabled Bolt will allow this entity to perform its first replication even if its frozen.");

    entity._detachOnDisable = EditorGUILayout.Toggle("Detach On Disable", entity._detachOnDisable);
    HelpBox("If enabled this entity will be detached from the network when its disabled.");

    entity._sceneObjectAutoAttach = EditorGUILayout.Toggle("Auto Attach On Load", entity._sceneObjectAutoAttach);
    HelpBox("If enabled this to automatically attach scene entities on map load.");

    entity._autoFreezeProxyFrames = EditorGUILayout.IntField("Auto Freeze Frames", entity._autoFreezeProxyFrames);
    HelpBox("If larger than 0, this entity will be automatically frozen by Bolt for non-owners if it has not received a network update for the amount of frames specified.");

    entity._clientPredicted = EditorGUILayout.Toggle("Controller Predicted Movement", entity._clientPredicted);
    HelpBox("If enabled this tells Bolt that this entity is using commands for moving and that they are applied on both the owner and controller.");

    entity._autoRemoveChildEntities = EditorGUILayout.Toggle("Remove Parent On Detach", entity._autoRemoveChildEntities);
    HelpBox("If enabled this tells Bolt to search the entire transform hierarchy of the entity being detached for nested entities and set their transform.parent to null.");

    EditorGUILayout.LabelField("Scene ID", entity._sceneGuid);
    HelpBox("The scene id of this entity");

    if (settings.clientCanInstantiateAll == false) {
      entity._allowInstantiateOnClient = EditorGUILayout.Toggle("Allow Client Instantiate", entity._allowInstantiateOnClient);
      HelpBox("If enabled this prefab can be instantiated by clients, this option can be globally enabled/disabled by changing the 'Instantiate Mode' setting in the 'Window/Bolt/Settings' window");
    }

    if (BoltNetwork.isRunning) {
      RuntimeInfoGUI(entity);
    }
  }

  public  void OnInspectorGUI2() {
    GUILayout.Space(4);

    BoltEditorGUI.Help("Entity Settings", "https://doc.photonengine.com/en/bolt/current/in-depth/entity-settings");

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

#if DEBUG
    EditorGUILayout.LabelField("Prefab Type", prefabType.ToString());
#endif

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

    EditState(entity);
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

  void DrawPrefabInfo(BoltEntity entity) {
    PrefabType prefabType = PrefabUtility.GetPrefabType(entity.gameObject);

#if DEBUG
    EditorGUILayout.LabelField("Type", prefabType.ToString());
    EditorGUILayout.LabelField("Scene Id", entity.sceneGuid.ToString());
#endif

    switch (prefabType) {
      case PrefabType.Prefab:
      case PrefabType.PrefabInstance:
        EditorGUILayout.LabelField("Id", entity._prefabId.ToString());

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
  }

  void EditState(BoltEntity entity) {
    BoltRuntimeSettings settings = BoltRuntimeSettings.instance;

    int selectedIndex;

    selectedIndex = Math.Max(0, Array.IndexOf(serializerIds, entity.serializerGuid) + 1);
    selectedIndex = EditorGUILayout.Popup("State", selectedIndex, serializerNames);

    if (selectedIndex == 0) {
      entity.serializerGuid = Bolt.UniqueId.None;
      EditorGUILayout.HelpBox("You must assign a state to this prefab before using it", MessageType.Error);
    }
    else {
      entity.serializerGuid = serializerIds[selectedIndex - 1];
    }

  }


  void EditProperties(BoltEntity entity) {
    BoltRuntimeSettings settings = BoltRuntimeSettings.instance;

    // Update Rate
    entity._updateRate = EditorGUILayout.IntField("Update Rate", entity._updateRate);
    entity._persistThroughSceneLoads = EditorGUILayout.Toggle("Persist Through Load", entity._persistThroughSceneLoads);
    entity._alwaysProxy = EditorGUILayout.Toggle("Always Proxy", entity._alwaysProxy);
    entity._detachOnDisable = EditorGUILayout.Toggle("Detach On Disable", entity._detachOnDisable);
    entity._allowFirstReplicationWhenFrozen = EditorGUILayout.Toggle("Allow Replication When Frozen", entity._allowFirstReplicationWhenFrozen);

    entity._autoFreezeProxyFrames = EditorGUILayout.IntField("Auto Freeze Frames", entity._autoFreezeProxyFrames);

    entity._clientPredicted = EditorGUILayout.Toggle("Controller Prediction", entity._clientPredicted);
    // Bool Settings


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
        EditorGUILayout.LabelField("Source", entity.source.RemoteEndPoint.ToString());
      }
      else {
        EditorGUILayout.LabelField("Source", "Local");
      }

      if (entity.controller != null) {
        EditorGUILayout.LabelField("Controller", entity.controller.RemoteEndPoint.ToString());
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


