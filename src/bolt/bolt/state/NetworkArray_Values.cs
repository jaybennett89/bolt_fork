using System;

namespace Bolt {
  public abstract class NetworkArray_Values<T> : NetworkObj {
    int _length;
    int _stride;

    public int Length {
      get { return _length; }
    }

    internal override NetworkStorage Storage {
      get { return Root.Storage; }
    }

    internal NetworkArray_Values(int length, int stride)
      : base(NetworkArray_Meta.Instance) {
      _length = length;
      _stride = stride;
    }

    public T this[int index] {
      get {
        if (index < 0 || index >= _length) {
          throw new IndexOutOfRangeException();
        }

        return GetValue(this.OffsetStorage + index);
      }
      set {
        if (index < 0 || index >= _length) {
          throw new IndexOutOfRangeException();
        }

        SetValue(this.OffsetStorage + (index * _stride), value);

        // set changed
        Storage.PropertyChanged(this.OffsetProperties + (index * _stride));
      }
    }

    protected abstract T GetValue(int index);
    protected abstract void SetValue(int index, T value);
  }
}
