using System;
using System.Collections.Generic;
using Bolt;

internal class EntityProxyEnvelope : BoltObject, IDisposable {
  public int Frame;
  public EntityProxy Proxy = null;
  public ProxyFlags Flags;
  public List<Priority> Written = new List<Priority>();

  public void Dispose() {
    Proxy = null;
    Flags = Bolt.ProxyFlags.ZERO;

    Written.Clear();

    BoltEntityProxyEnvelopePool.Release(this);
  }
}

static class BoltEntityProxyEnvelopePool {
  static readonly Stack<EntityProxyEnvelope> pool = new Stack<EntityProxyEnvelope>();

  internal static EntityProxyEnvelope Acquire() {
    EntityProxyEnvelope obj;

    if (pool.Count > 0) {
      obj = pool.Pop();
    }
    else {
      obj = new EntityProxyEnvelope();
    }

    Assert.True(obj._pooled);
    obj._pooled = false;
    return obj;
  }

  internal static void Release(EntityProxyEnvelope obj) {
    Assert.False(obj._pooled);
    obj._pooled = true;
    pool.Push(obj);
  }
}
internal partial class EntityProxy : BoltObject {
  // ################### OLD


  //public const uint FLAG_CREATE = 1;
  //public const uint FLAG_CREATE_IN_PROGRESS = 2;
  //public const uint FLAG_DESTROY = 4;
  //public const uint FLAG_DESTROY_IN_PROGRESS = 8;
  //public const uint FLAG_IDLE = 16;
  //public const uint FLAG_FORCE_SYNC = 32;

  //public float priority = 0;
  //public uint networkId = 0;
  //public uint skipped = 0;
  //public Bits mask = 0;
  //public Bits flags = 0;

  //public bool destroyed = false;
  //public BoltEntity entity = null;
  //public BoltConnection connection = null;
  //public BoltRingBuffer<BoltEntityProxyEnvelope> envelopes;

  public const int ID_BIT_COUNT = 10;
  public const int MAX_COUNT = 1 << ID_BIT_COUNT;

  // ################### NEW

  public NetId NetId;
  public Bolt.State State;
  public Bolt.Filter Filter;
  public Bolt.BitArray Mask;
  public Bolt.ProxyFlags Flags;
  public Bolt.Priority[] PropertyPriority;

  public Bolt.EntityObject Entity;
  public BoltConnection Connection;
  public BoltRingBuffer<EntityProxyEnvelope> Envelopes;

  public int Skipped;
  public float Priority;

  // ###################

  public EntityProxy() {
    Envelopes = new BoltRingBuffer<EntityProxyEnvelope>(BoltCore._udpConfig.PacketWindow);
  }

  public EntityProxyEnvelope CreateEnvelope() {
    EntityProxyEnvelope env = BoltEntityProxyEnvelopePool.Acquire();
    env.Flags = Flags;
    env.Proxy = this;
    return env;
  }

  public override string ToString() {
    return string.Format("[Proxy {0} {1}]", NetId, ((object)Entity) ?? ((object)"NULL"));
  }
}

partial class EntityProxy : BoltObject {
  public class PriorityComparer : IComparer<EntityProxy> {
    public static readonly PriorityComparer Instance = new PriorityComparer();

    PriorityComparer() {

    }

    int IComparer<EntityProxy>.Compare(EntityProxy x, EntityProxy y) {
      return y.Priority.CompareTo(x.Priority);
    }
  }

  public static EntityProxy Alloc() {
    EntityProxy proxy = new EntityProxy();
    BoltCore._proxies.AddLast(proxy);
    return proxy;
  }

  public static void Free(EntityProxy proxy) {
    // mark as destroyed
    proxy.Flags |= Bolt.ProxyFlags.DESTROY_DONE;

    // remove from global list
    BoltCore._proxies.Remove(proxy);
  }

  public static implicit operator bool(EntityProxy proxy) {
    return proxy != null;
  }
}