using System.Collections.Generic;
using UE = UnityEngine;

namespace Bolt {
  public class PrefabDatabase : UE.ScriptableObject {
    static Dictionary<Bolt.PrefabId, UE.GameObject> lookup;

    static public PrefabDatabase Instance {
      get { return UE.Resources.Load("BoltPrefabDatabase", typeof(PrefabDatabase)) as PrefabDatabase; }
    }

    [UE.SerializeField]
    internal bool ManualMode = false;

    [UE.SerializeField]
    internal UE.GameObject[] Prefabs = new UE.GameObject[0];

    internal static UE.GameObject Find(Bolt.PrefabId id) {
      if (lookup == null) {
        lookup = new Dictionary<Bolt.PrefabId, UE.GameObject>();

        for (int i = 1; i < Instance.Prefabs.Length; ++i) {
          if (Instance.Prefabs[i]) {
            lookup.Add(Instance.Prefabs[i].GetComponent<BoltEntity>().prefabId, Instance.Prefabs[i]);
          }
        }
      }

      UE.GameObject prefab;

      if (lookup.TryGetValue(id, out prefab)) {
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
