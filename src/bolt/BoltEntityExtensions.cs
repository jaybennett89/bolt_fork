public static class BoltEntityExtensions {
  public static bool IsAttached(this BoltEntity entity) {
    return BoltNetwork.isRunning && entity && entity.isAttached;
  }

  public static bool IsOwner(this BoltEntity entity) {
    return entity.IsAttached() && entity.isOwner;
  }

  public static bool IsControlled(this BoltEntity entity) {
    return entity.IsAttached() && entity.isControlled;
  }

  public static bool IsSceneObject(this BoltEntity entity) {
    return entity.IsAttached() && entity.isSceneObject;
  }

  public static bool IsFrozen(this BoltEntity entity) {
    return entity.IsAttached() && entity.isFrozen;
  }

  public static bool HasControl(this BoltEntity entity) {
    return entity.IsAttached() && entity.hasControl;
  }
}
