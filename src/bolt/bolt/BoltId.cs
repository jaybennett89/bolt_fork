public struct BoltUniqueId {
  public readonly uint peer;
  public readonly uint obj;

  internal BoltUniqueId (uint peerId, uint objectId) {
    this.peer = peerId;
    this.obj = objectId;
  }

  public override string ToString () {
    return string.Format("[Id peer={0} object={1}]", peer, obj);
  }

  public static bool operator ==(BoltUniqueId l, BoltUniqueId r) {
    return l.peer == r.peer && l.obj == r.obj;
  }

  public static bool operator !=(BoltUniqueId l, BoltUniqueId r) {
    return l.peer != r.peer || l.obj != r.obj;
  }
}