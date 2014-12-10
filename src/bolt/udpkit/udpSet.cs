using System.Collections.Generic;

namespace UdpKit {
  class UdpSet<T> {
    readonly Dictionary<T, object> set;

    public int Count {
      get { return set.Count; }
    }

    public bool Remove(T value) {
      return set.Remove(value);
    }

    public void Clear() {
      set.Clear();
    }

    public UdpSet(IEqualityComparer<T> comparer) {
      set = new Dictionary<T, object>(comparer);
    }

    public bool Add(T value) {
      if (set.ContainsKey(value)) {
        return false;
      }

      set.Add(value, null);
      return true;
    }

    public bool Contains(T value) {
      return set.ContainsKey(value);
    }
  }
}
