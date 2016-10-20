//using System.Collections.Generic;

//namespace Bolt {
//  class NetIdPool {
//    Stack<NetId> pool;

//    public int Available {
//      get { return pool.Count; }
//    }

//    public NetIdPool(int maxCount) {
//      pool = new Stack<NetId>(maxCount);

//      for (int id = 0; id < maxCount; ++id) {
//        pool.Push(new NetId(id));
//      }
//    }

//    public bool Acquire(out NetId index) {
//      lock (pool) {
//        if (pool.Count > 0) {
//          index = pool.Pop();
//          return true;
//        }
//      }

//      index = new NetId(int.MaxValue);
//      return false;
//    }

//    public void Release(NetId index) {
//      lock (pool) {
//        pool.Push(index);
//      }
//    }
//  }

//}