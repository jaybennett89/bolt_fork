//using System;

//namespace Bolt {
//  public struct Slice<T> where T : struct {
//    readonly int offset;
//    readonly int length;
//    readonly byte[] storage;

//    public Slice(byte[] storage, int offset, int length) {
//      this.offset = offset;
//      this.length = length;
//      this.storage = storage;
//    }

//    public T this[int index] {
//      get {
//        throw new ArgumentOutOfRangeException("index");
//      }
//    }
//  }
//}