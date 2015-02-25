using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  public class EntityLookup : IEnumerable<BoltEntity> {
    readonly Dictionary<Bolt.NetworkId, EntityProxy> _dict;

    internal EntityLookup(Dictionary<Bolt.NetworkId, EntityProxy> d) {
      _dict = d;
    }

    public bool TryGet(Bolt.NetworkId id, out BoltEntity entity) {
      EntityProxy proxy;

      if (_dict.TryGetValue(id, out proxy) && proxy.Entity != null && proxy.Entity.UnityObject != null) {
        entity = proxy.Entity.UnityObject;
        return true;
      }

      entity = null;
      return false;
    }

    public IEnumerator<BoltEntity> GetEnumerator() {
      foreach (var proxy in _dict.Values) {
        if (proxy != null && proxy.Entity != null && proxy.Entity.UnityObject != null) {
          yield return proxy.Entity.UnityObject;
        }
      }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }
  }
}
