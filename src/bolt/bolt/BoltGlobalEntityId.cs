public struct BoltPeerId {
  public readonly uint id;

  public BoltPeerId (uint id) {
    this.id = id;
  }
}

public struct BoltObjectId {
  public readonly BoltPeerId peer;
  public readonly uint entityId;

  public BoltObjectId (BoltPeerId peer, uint obj) {
    this.peer = peer;
    this.entityId = obj;
  }
}
