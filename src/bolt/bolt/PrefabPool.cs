using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UE = UnityEngine;

namespace Bolt {
  public interface IPrefabPool {
    /// <summary>
    /// Called by Bolt to inspect a prefab before instantiating it. The object
    /// returned from this method can be the prefab itself, it does not have
    /// to be a unique instance.
    /// </summary>
    /// <param name="prefabId">The id of the prefab we are looking for</param>
    /// <returns>A game object representing the prefab or an instance of the prefab</returns>
    UE.GameObject LoadPrefab(Bolt.PrefabId prefabId);

    /// <summary>
    /// This is called when bolt wants to create a new instance of an entity prefab.
    /// </summary>
    /// <param name="prefabId">The id of this prefab</param>
    /// <param name="position">The position we want the instance instantiated at</param>
    /// <param name="rotation">The rotation we want the instance to take</param>
    /// <returns>The newly instantiate object, or null if a prefab with <paramref name="prefabId"/> was not found</returns>
    UE.GameObject Instantiate(Bolt.PrefabId prefabId, UE.Vector3 position, UE.Quaternion rotation);

    /// <summary>
    /// This is called when Bolt wants to destroy the instance of an entity prefab.
    /// </summary>
    /// <param name="gameObject">The instance to destroy</param>
    void Destroy(UE.GameObject gameObject);
  }

  public class DefaultPrefabPool : IPrefabPool {
    UE.GameObject IPrefabPool.Instantiate(PrefabId prefabId, UE.Vector3 position, UE.Quaternion rotation) {
      return (UE.GameObject)UE.GameObject.Instantiate(((IPrefabPool)this).LoadPrefab(prefabId), position, rotation);
    }

    UE.GameObject IPrefabPool.LoadPrefab(PrefabId prefabId) {
      return BoltRuntimeSettings.FindPrefab(prefabId);
    }

    void IPrefabPool.Destroy(UE.GameObject gameObject) {
      UE.GameObject.Destroy(gameObject);
    }
  }
}
