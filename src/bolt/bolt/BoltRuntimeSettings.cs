using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BoltEditorStartMode {
  None = 0,
  Server = 1,
  Client = 2
}

public class BoltRuntimeSettings : ScriptableObject {
  static BoltRuntimeSettings _instance;

  public static BoltRuntimeSettings instance {
    get {
      if (_instance == null) {
        _instance = (BoltRuntimeSettings)Resources.Load(typeof(BoltRuntimeSettings).Name, typeof(BoltRuntimeSettings));

        if (_instance == null) {
          BoltLog.Error("Could not find resource: '{0}' ", typeof(BoltRuntimeSettings));
        }
      }

      return _instance;
    }
  }

  [SerializeField]
  internal GameObject[] _prefabs = new GameObject[0];

  [SerializeField]
  internal BoltConfig _config = new BoltConfig();

  [SerializeField]
  public int debugClientCount = 1;

  [SerializeField]
  public int debugStartPort = 54321;

  [SerializeField]
  public string debugStartMapName = null;

  [SerializeField]
  public bool debugPlayAsServer = false;

  [SerializeField]
  public BoltEditorStartMode debugEditorMode = BoltEditorStartMode.Server;

  [SerializeField]
  public KeyCode consoleToggleKey = KeyCode.Tab;

  [SerializeField]
  public bool consoleVisibleByDefault = true;

  [SerializeField]
  public int compilationWarnLevel = 4;

  [SerializeField]
  public int editorSkin = 4;

  [SerializeField]
  public Color _notUsed = new Color(81f / 255f, 203f / 255f, 255f / 255f);

  [SerializeField]
  public string projectPath = "";

  public BoltConfig GetConfigCopy() {
    return _config.Clone();
  }


  static Dictionary<Bolt.PrefabId, GameObject> prefabLookup = null;

  static GameObject[] prefabsArray {
    get {
      if (!instance)
        return new GameObject[0];

      return instance._prefabs;
    }
  }

  internal static GameObject FindPrefab(Bolt.PrefabId id) {
    if (prefabLookup == null) {
      prefabLookup = new Dictionary<Bolt.PrefabId, GameObject>();

      for (int i = 0; i < prefabsArray.Length; ++i) {
        Bolt.PrefabId prefabId = new Bolt.PrefabId(prefabsArray[i].GetComponent<BoltEntity>()._prefabId);
        prefabLookup.Add(prefabId, prefabsArray[i]);
      }
    }

    GameObject go;

    if (prefabLookup.TryGetValue(id, out go)) {
      return go;
    }
    else {
      BoltLog.Error("Could not find game object for {0}", id);
      return null;
    }
  }


  internal static bool ContainsPrefab(BoltEntity entity) {
    if (prefabsArray == null)
      return false;

    if (!entity)
      return false;

    if (entity._prefabId >= BoltRuntimeSettings.prefabsArray.Length)
      return false;

    if (entity._prefabId < 0)
      return false;

    return BoltRuntimeSettings.prefabsArray[entity._prefabId] == entity.gameObject;
  }
}
