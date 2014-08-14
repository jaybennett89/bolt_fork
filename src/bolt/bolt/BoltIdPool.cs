using System.Collections.Generic;

class BoltIdPool {
    Stack<uint> pool;

    public int Available {
        get { return pool.Count; }
    }

    public BoltIdPool (int maxCount) {
        pool = new Stack<uint>(maxCount);

        for (uint id = 0; id < maxCount; ++id) {
            pool.Push(id);
        }
    }

    public bool Acquire (out uint index) {
        lock (pool) {
            if (pool.Count > 0) {
                index = pool.Pop();
                return true;
            }
        }

        index = uint.MaxValue;
        return false;
    }

    public void Release (uint index) {
        lock (pool) {
            pool.Push(index);
        }
    }
}
