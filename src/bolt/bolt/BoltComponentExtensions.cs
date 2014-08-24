using UnityEngine;

public static class BoltComponentExtensions {

  public static BoltEntity GetBoltEntity (this Component mb) {
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

  public static BoltEntitySerializer GetBoltSerializer (this Component mb) {
    return mb.GetBoltEntity().boltSerializer;
  }

  public static TState GetBoltState<TState> (this Component mb) where TState : class, IBoltState {
    return ((BoltEntitySerializer<TState>) mb.GetBoltSerializer()).boltState;
  }
}