public struct BoltUniqueId {
  public readonly uint peer;
  public readonly uint obj;

  internal BoltUniqueId (uint peerId, uint objectId) {
    this.peer = peerId;
    this.obj = objectId;
  }

  public static bool operator ==(BoltUniqueId l, BoltUniqueId r) {
    return l.peer == r.peer && l.obj == r.obj;
  }

  public static bool operator !=(BoltUniqueId l, BoltUniqueId r) {
    return l.peer != r.peer || l.obj != r.obj;
  }
}