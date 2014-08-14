using System.Collections.Generic;

namespace UdpKit {
    class UdpSet<T> {
        public int Count {
            get { return set.Count; }
        }

        public bool Remove (T value) {
            return set.Remove(value);
        }

        public void Clear () {
            set.Clear();
        }

#if HAS_HASHSET
        readonly HashSet<T> set;

        public UdpSet (IEqualityComparer<T> comparer) {
            set = new HashSet<T>(comparer);
        }

        public bool Add (T value) {
            return set.Add(value);
        }

        public bool Contains (T value) {
            return set.Contains(value);
        }
#else
        readonly Dictionary<T, object> set;

        public UdpSet (IEqualityComparer<T> comparer) {
            set = new Dictionary<T, object>(comparer);
        }

        public bool Add (T value) {
            if (set.ContainsKey(value))
                return false;

            set.Add(value, null);
            return true;
        }

        public bool Contains (T value) {
            return set.ContainsKey(value);
        }
#endif
    }
}
