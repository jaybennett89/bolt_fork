using System;
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
        _instance = (BoltRuntimeSettings) Resources.Load(typeof(BoltRuntimeSettings).Name, typeof(BoltRuntimeSettings));

        if (_instance == null) {
          BoltLog.Error("could not find {0} asset", typeof(BoltRuntimeSettings));
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
  public BoltEditorStartMode debugEditorMode = BoltEditorStartMode.Server;

  [SerializeField]
  public KeyCode consoleToggleKey = KeyCode.Tab;

  [SerializeField]
  public bool consoleVisibleByDefault = true;

  [SerializeField]
  public int compilationWarnLevel = 4;

  public BoltConfig GetConfigCopy () {
    return _config.Clone();
  }

  internal static GameObject[] prefabs {
    get {
      if (!instance)
        return new GameObject[0];

      return instance._prefabs;
    }
  }

  internal static GameObject FindPrefab (string name) {
    return prefabs.FirstOrDefault(x => x.name == name);
  }

  internal static bool ContainsPrefab (BoltEntity entity) {
    if (prefabs == null)
      return false;

    if (!entity)
      return false;

    if (entity._prefabId >= BoltRuntimeSettings.prefabs.Length)
      return false;

    if (entity._prefabId < 0)
      return false;

    return BoltRuntimeSettings.prefabs[entity._prefabId] == entity.gameObject;
  }
}
