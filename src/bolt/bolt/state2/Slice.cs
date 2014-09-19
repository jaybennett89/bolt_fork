using System;

public struct Slice<T> where T : class {
  readonly int offset;
  readonly int length;
  readonly object[] storage;

  public Slice(object[] storage, int offset, int length) {
    this.offset = offset;
    this.length = length;
    this.storage = storage;
  }

  public T this[int index] {
    get {
      if ((index < 0) || (index >= length)) {
        throw new ArgumentOutOfRangeException("index");
      }

      return (T)storage[offset + index];
    }
  }
}
