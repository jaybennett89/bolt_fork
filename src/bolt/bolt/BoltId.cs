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
  public readonly BoltPeerId peerId;
  public readonly BoltObjectId objectId;

  internal BoltUniqueId (BoltPeerId peerId, BoltObjectId objectId) {
    this.peerId = peerId;
    this.objectId = objectId;
  }
}