using System.Collections.Generic;
using UE = UnityEngine;

namespace Bolt {
  public enum PrefabDatabaseMode {
    AutomaticScan = 0,
    ManualScan = 1,
    Manual = 2
  }

  public class PrefabDatabase : UE.ScriptableObject {
    static PrefabDatabase _instance;
    static Dictionary<Bolt.PrefabId, UE.GameObject> _lookup;

    public static PrefabDatabase Instance {
      get {
        if (_instance == null) {
          _instance = (PrefabDatabase)UE.Resources.Load("BoltPrefabDatabase", typeof(PrefabDatabase));

          if (_instance == null) {
            BoltLog.Error("Could not find resource 'BoltPrefabDatabase'");
          }
        }

        return _instance;
      }
    }

    [UE.SerializeField]
    internal PrefabDatabaseMode DatabaseMode = PrefabDatabaseMode.AutomaticScan;

    [UE.SerializeField]
    internal UE.GameObject[] Prefabs = new UE.GameObject[0];

    internal static void BuildCache() {
      LoadInstance();
      UpdateLookup();
    }

    static void UpdateLookup() {
      _lookup = new Dictionary<Bolt.PrefabId, UE.GameObject>();

      for (int i = 1; i < Instance.Prefabs.Length; ++i) {
        if (Instance.Prefabs[i]) {
          var prefabId = Instance.Prefabs[i].GetComponent<BoltEntity>().prefabId;

          if (_lookup.ContainsKey(prefabId)) {
            throw new BoltException("Duplicate {0} for {1} and {2}", prefabId, Instance.Prefabs[i].GetComponent<BoltEntity>(), _lookup[prefabId].GetComponent<BoltEntity>());
          }

          _lookup.Add(Instance.Prefabs[i].GetComponent<BoltEntity>().prefabId, Instance.Prefabs[i]);
        }
      }
    }

    static void LoadInstance() {
      _instance = (PrefabDatabase)UE.Resources.Load("BoltPrefabDatabase", typeof(PrefabDatabase));
    }

    public static UE.GameObject Find(Bolt.PrefabId id) {
      if (_lookup == null || _instance == null) {
        LoadInstance();
        UpdateLookup();
      }

      UE.GameObject prefab;

      if (_lookup.TryGetValue(id, out prefab)) {
        return prefab;
      }
      else {
        BoltLog.Error("Could not find game object for {0}", id);
        return null;
      }
    }

    internal static bool Contains(BoltEntity entity) {
      if (Instance.Prefabs == null)
        return false;

      if (!entity)
        return false;

      if (entity._prefabId >= Instance.Prefabs.Length)
        return false;

      if (entity._prefabId < 0)
        return false;

      return Instance.Prefabs[entity._prefabId] == entity.gameObject;
    }
  }
}
