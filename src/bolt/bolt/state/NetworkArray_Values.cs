using System;

namespace Bolt {
  public abstract class NetworkArray_Values<T> : NetworkObj {
    int _length;

    public int Length {
      get { return _length; }
    }

    internal override NetworkStorage Storage {
      get { return Root.Storage; }
    }

    internal NetworkArray_Values(int length)
      : base(NetworkArray_Meta.Instance) {
      _length = length;
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

        SetValue(this.OffsetStorage + index, value);

        // set changed
        Storage.PropertyChanged(this.OffsetProperties + index);
      }
    }

    protected abstract T GetValue(int index);
    protected abstract void SetValue(int index, T value);
  }
}
