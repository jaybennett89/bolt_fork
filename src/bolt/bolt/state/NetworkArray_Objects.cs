using System;

namespace Bolt {
  [Documentation]
  public class NetworkArray_Objects<T> : NetworkObj where T : NetworkObj {
    int _length;

    public int Length {
      get { return _length; }
    }

    internal override NetworkStorage Storage {
      get { return Root.Storage; }
    }

    internal NetworkArray_Objects(int length)
      : base(NetworkArray_Meta.Instance) {
      _length = length;
    }

    public T this[int index] {
      get {
        if (index < 0 || index >= _length) {
          throw new IndexOutOfRangeException();
        }
        
        return (T)Objects[this.OffsetObjects + index];
      }
    }

  }
}