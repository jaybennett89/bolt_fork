using System;

namespace UdpKit {
  public class UdpRingBuffer<T> : System.Collections.Generic.IEnumerable<T> {
    int _head;
    int _tail;
    int _count;
    bool _autofree;

    readonly T[] array;

    public bool Full {
      get { return _count == array.Length; }
    }

    public float FillRatio {
      get { return UdpMath.Clamp((float)_count / (float)array.Length, 0, 1f); }
    }

    public bool Empty {
      get { return _count == 0; }
    }

    public bool AutoFree {
      get { return _autofree; }
      set { _autofree = value; }
    }

    public int Count {
      get { return _count; }
    }

    public T First {
      get {
        VerifyNotEmpty();
        return this[0];
      }
      set {
        VerifyNotEmpty();
        this[0] = value;
      }
    }

    public T FirstOrDefault {
      get {
        if (Count > 0) {
          return First;
        }

        return default(T);
      }
    }

    public T Last {
      get {
        VerifyNotEmpty();
        return this[Count - 1];
      }
      set {
        VerifyNotEmpty();
        this[Count - 1] = value;
      }
    }

    public T LastOrDefault {
      get {
        if (Count > 0) {
          return Last;
        }

        return default(T);
      }
    }

    public T this[int index] {
      get {
        VerifyNotEmpty();
        return array[(_tail + index) % array.Length];
      }
      set {
        if (index >= _count) {
          throw new IndexOutOfRangeException("can't change value of non-existand index");
        }

        array[(_tail + index) % array.Length] = value;
      }
    }

    public UdpRingBuffer(int size) {
      array = new T[size];
    }

    public void Enqueue(T item) {
      if (_count == array.Length) {
        if (_autofree) {
          Dequeue();
        }
        else {
          throw new InvalidOperationException("buffer is full");
        }
      }

      array[_head] = item;
      _head = (_head + 1) % array.Length;
      _count += 1;
    }

    public T Dequeue() {
      VerifyNotEmpty();
      T item = array[_tail];
      array[_tail] = default(T);
      _tail = (_tail + 1) % array.Length;
      _count -= 1;
      return item;
    }

    public T Peek() {
      VerifyNotEmpty();
      return array[_tail];
    }

    public void Clear() {
      Array.Clear(array, 0, array.Length);
      _count = _tail = _head = 0;
    }

    public void CopyTo(UdpRingBuffer<T> other) {
      if (this.array.Length != other.array.Length) {
        throw new InvalidOperationException("buffers must be of the same capacity");
      }

      other._head = this._head;
      other._tail = this._tail;
      other._count = this._count;

      Array.Copy(this.array, 0, other.array, 0, this.array.Length);
    }

    void VerifyNotEmpty() {
      if (_count == 0) {
        throw new InvalidOperationException("buffer is empty");
      }
    }

    public System.Collections.Generic.IEnumerator<T> GetEnumerator() {
      for (int i = 0; i < _count; ++i) {
        yield return this[i];
      }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return this.GetEnumerator();
    }
  }

}
