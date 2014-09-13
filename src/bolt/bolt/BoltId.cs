﻿public struct BoltUniqueId {
  public readonly uint peer;
  public readonly uint entity;

  internal BoltUniqueId(uint peer, uint entity) {
    this.peer = peer;
    this.entity = entity;
  }

  public override int GetHashCode() {
    return (int)(this.peer ^ this.entity);
  }

  public override bool Equals(object obj) {
    if (obj is BoltUniqueId) {
      return this == ((BoltUniqueId)obj);
    }

    return false;
  }

  public override string ToString() {
    if (peer == 0) {
      Assert.True(entity == 0);
      return "[Id NULL]";
    }

    return string.Format("[Id peer={0} entity={1}]", peer, entity);
  }

  public static bool operator ==(BoltUniqueId l, BoltUniqueId r) {
    return l.peer == r.peer && l.entity == r.entity;
  }

  public static bool operator !=(BoltUniqueId l, BoltUniqueId r) {
    return l.peer != r.peer || l.entity != r.entity;
  }
}