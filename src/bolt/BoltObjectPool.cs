using System.Collections.Generic;
using Bolt;

[Documentation(Ignore = true)]
class BoltObjectPool<T> where T : BoltObject, new() {
  readonly Stack<T> _pool = new Stack<T>();

  public T Acquire () {
    T obj;

    if (_pool.Count > 0) {
      obj = _pool.Pop();
    } else {
      obj = new T();
    }

#if DEBUG
    Assert.True(obj._pooled);
    obj._pooled = false;
#endif

    return obj;
  }

  public void Release (T obj) {
#if DEBUG
    Assert.False(obj._pooled);
    obj._pooled = true;
#endif

    _pool.Push(obj);
  }
}
