using System;

namespace Bolt {
  [Documentation]
  public class NetworkArray_Objects<T> : NetworkObj where T : NetworkObj {
    int _length;
    int _stride;

    public int Length {
      get { return _length; }
    }

    internal override NetworkStorage Storage {
      get { return Root.Storage; }
    }

    internal NetworkArray_Objects(int length, int stride)
      : base(NetworkArray_Meta.Instance) {
      _length = length;
      _stride = stride;
    }

    public T this[int index] {
      get {
        if (index < 0 || index >= _length) {
          throw new IndexOutOfRangeException();
        }

        return (T)Objects[this.OffsetObjects + 1 + (index * _stride)];
      }
    }

  }
}