using System;
using System.Collections.Generic;
using Bolt;

internal class EntityProxyEnvelope : BoltObject, IDisposable {
  public int PacketNumber;
  public ProxyFlags Flags;
  public EntityProxy Proxy = null;
  public List<Priority> Written = new List<Priority>();

  public IProtocolToken ControlTokenLost;
  public IProtocolToken ControlTokenGained;

  public void Dispose() {
    Proxy = null;
    Flags = Bolt.ProxyFlags.ZERO;

    Written.Clear();

    EntityProxyEnvelopePool.Release(this);
  }
}

static class EntityProxyEnvelopePool {
  static readonly Stack<EntityProxyEnvelope> pool = new Stack<EntityProxyEnvelope>();

  internal static EntityProxyEnvelope Acquire() {
    EntityProxyEnvelope obj;

    if (pool.Count > 0) {
      obj = pool.Pop();
    }
    else {
      obj = new EntityProxyEnvelope();
    }

#if DEBUG
    Assert.True(obj._pooled);
    obj._pooled = false;
#endif
    return obj;
  }

  internal static void Release(EntityProxyEnvelope obj) {
#if DEBUG
    Assert.False(obj._pooled);
    obj._pooled = true;
#endif
    pool.Push(obj);
  }
}

internal partial class EntityProxy : BoltObject {
  public class PriorityComparer : IComparer<EntityProxy> {
    public static readonly PriorityComparer Instance = new PriorityComparer();

    PriorityComparer() {

    }

    int IComparer<EntityProxy>.Compare(EntityProxy x, EntityProxy y) {
      return y.Priority.CompareTo(x.Priority);
    }
  }

  public Bolt.NetworkId NetworkId;
  public Bolt.State State;
  public Bolt.Filter Filter;
  public Bolt.BitSet Changed;
  public Bolt.ProxyFlags Flags;
  public Bolt.Priority[] PropertyPriority;

  public Bolt.Entity Entity;
  public BoltConnection Connection;
  public Queue<EntityProxyEnvelope> Envelopes;

  public IProtocolToken ControlTokenLost;
  public IProtocolToken ControlTokenGained;

  public int Skipped;
  public float Priority;

  // ###################

  public EntityProxy() {
    Envelopes = new Queue<EntityProxyEnvelope>();
  }

  public EntityProxyEnvelope CreateEnvelope() {
    EntityProxyEnvelope env = EntityProxyEnvelopePool.Acquire();
    env.Proxy = this;
    env.Flags = this.Flags;
    env.ControlTokenLost = this.ControlTokenLost;
    env.ControlTokenGained = this.ControlTokenGained;
    return env;
  }

  public override string ToString() {
    return string.Format("[Proxy {0} {1}]", NetworkId, ((object)Entity) ?? ((object)"NULL"));
  }
}
