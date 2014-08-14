using System;
using System.Collections.Generic;

class BoltEntityProxyEnvelope : BoltObject, IDisposable {
  public bool pooled = true;
  public Bits mask = 0;
  public Bits flags = 0;
  public BoltEntityProxy proxy = null;

  public void Dispose () {
    mask = 0;
    flags = 0;
    proxy = null;
    BoltEntityProxyEnvelopePool.Release(this);
  }
}

static class BoltEntityProxyEnvelopePool {
  static readonly Stack<BoltEntityProxyEnvelope> pool = new Stack<BoltEntityProxyEnvelope>();

  internal static BoltEntityProxyEnvelope Acquire () {
    BoltEntityProxyEnvelope obj;

    if (pool.Count > 0) {
      obj = pool.Pop();
    } else {
      obj = new BoltEntityProxyEnvelope();
    }

    Assert.True(obj.pooled);
    obj.pooled = false;
    return obj;
  }

  internal static void Release (BoltEntityProxyEnvelope obj) {
    Assert.False(obj.pooled);
    obj.pooled = true;
    pool.Push(obj);
  }
}

class BoltEntityProxy : BoltObject {
  public class PriorityComparer : IComparer<BoltEntityProxy> {
    public static readonly PriorityComparer Instance = new PriorityComparer();

    PriorityComparer () {

    }

    int IComparer<BoltEntityProxy>.Compare (BoltEntityProxy x, BoltEntityProxy y) {
      return y.priority.CompareTo(x.priority);
    }
  }

  public static BoltEntityProxy Alloc () {
    BoltEntityProxy proxy = new BoltEntityProxy();
    BoltCore._proxies.AddLast(proxy);
    return proxy;
  }

  public static void Free (BoltEntityProxy proxy) {
    // mark as destroyed
    proxy.destroyed = true;

    // remove from global list
    BoltCore._proxies.Remove(proxy);
  }

  public const uint FLAG_CREATE = 1;
  public const uint FLAG_CREATE_IN_PROGRESS = 2;
  public const uint FLAG_DESTROY = 4;
  public const uint FLAG_DESTROY_IN_PROGRESS = 8;
  public const uint FLAG_IDLE = 16;
  public const uint FLAG_FORCE_SYNC = 32;

  public const int ID_BIT_COUNT = 8;
  public const int MAX_COUNT = 1 << ID_BIT_COUNT;

  public float priority = 0;
  public uint networkId = 0;
  public uint skipped = 0;
  public Bits mask = 0;
  public Bits flags = 0;
  public bool destroyed = false;

  public BoltEntity entity = null;
  public BoltConnection connection = null;
  public BoltRingBuffer<BoltEntityProxyEnvelope> envelopes;

  public BoltEntityProxy () {
    envelopes = new BoltRingBuffer<BoltEntityProxyEnvelope>(BoltCore.udpConfig.PacketWindow);
  }

  public BoltEntityProxyEnvelope CreateEnvelope () {
    BoltEntityProxyEnvelope env = BoltEntityProxyEnvelopePool.Acquire();
    env.flags = flags;
    env.proxy = this;
    env.mask = mask;
    return env;
  }

  public override string ToString () {
    return string.Format("[EntityProxy id={0} entity={1}]", networkId, ((object) entity) ?? ((object) "NULL"));
  }

  public static implicit operator bool (BoltEntityProxy proxy) {
    return proxy != null;
  }
}
