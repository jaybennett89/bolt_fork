using UnityEngine;

public static class BoltMonoBehaviourExtensions {

  public static BoltEntity GetBoltEntity (this MonoBehaviour mb) {
    BoltEntity entity = null;
    Transform t = mb.transform;

    while (t && !entity) {
      entity = t.GetComponent<BoltEntity>();
      t = t.parent;
    }

    if (!entity) {
      BoltLog.Error("could not find entity attached to '{0}' or any of it's parents", mb.gameObject.name);
    }

    return entity;
  }

  public static BoltEntitySerializer GetBoltSerializer (this MonoBehaviour mb) {
    return mb.GetBoltEntity().boltSerializer;
  }

  public static TState GetBoltState<TState> (this MonoBehaviour mb) where TState : class, IBoltState {
    return ((BoltEntitySerializer<TState>) mb.GetBoltSerializer()).boltState;
  }
}