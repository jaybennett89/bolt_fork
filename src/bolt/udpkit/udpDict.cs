using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit {
  class UdpDict<TKey, TVal> {
    Dictionary<TKey, TVal> dictionary;

    public UdpDict(IEqualityComparer<TKey> comparer) {
      dictionary = new Dictionary<TKey, TVal>(comparer);
    }

    public void Add(TKey key, TVal val) {
      lock (dictionary) {
        dictionary.Add(key, val);
      }
    }

    public bool Remove(TKey key) {
      lock (dictionary) {
        return dictionary.Remove(key);
      }
    }

    public bool ContainsKey(TKey key) {
      lock (dictionary) {
        return dictionary.ContainsKey(key);
      }
    }

    public bool TryGetValue(TKey key, out TVal val) {
      lock (dictionary) {
        return dictionary.TryGetValue(key, out val);
      }
    }
  }
}
