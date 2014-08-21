public struct BoltPeerId {
  public readonly uint id;

  internal BoltPeerId (uint id) {
    this.id = id;
  }
}

public struct BoltObjectId {
  public readonly uint id;

  internal BoltObjectId (uint id) {
    this.id = id;
  }
}

public struct BoltUniqueId {
  public readonly BoltPeerId peer;
  public readonly BoltObjectId obj;

  internal BoltUniqueId (BoltPeerId peerId, BoltObjectId objectId) {
    this.peer = peerId;
    this.obj = objectId;
  }

  public static bool operator ==(BoltUniqueId l, BoltUniqueId r) {
    return l.peer.id == r.peer.id && l.obj.id == r.obj.id;
  }

  public static bool operator !=(BoltUniqueId l, BoltUniqueId r) {
    return l.peer.id != r.peer.id || l.obj.id != r.obj.id;
  }
}