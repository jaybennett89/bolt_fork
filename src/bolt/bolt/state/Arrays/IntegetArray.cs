using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public struct IntegerArray {
  internal Bolt.State.Frame frame;
  internal int offsetObjects;
  internal int offsetBytes;
  internal int length;

  internal IntegerArray(Bolt.State.Frame frame, int offsetBytes, int offsetObjects, int length) {
    this.frame = frame;
    this.offsetBytes = offsetBytes;
    this.offsetObjects = offsetObjects;
    this.length = length;
  }

  public int Length {
    get {
      return length;
    }
  }

  public int this[int index] {
    get {
      if (index < 0 || index >= length) throw new IndexOutOfRangeException();
      return Bolt.Blit.ReadI32(frame.Data, offsetBytes + (index * 4));
    }
  }

  public IntegerArrayModifier Modify() {
    return new IntegerArrayModifier(this);
  }
}

public class IntegerArrayModifier : IDisposable {
  IntegerArray array;

  internal IntegerArrayModifier(IntegerArray array) {
    this.array = array;
  }

  public int Length {
    get {
      return array.length;
    }
  }

  public int this[int index] {
    get { return array[index]; }
    set {
      if (index < 0 || index >= array.length) throw new IndexOutOfRangeException();
      Bolt.Blit.PackI32(array.frame.Data, array.offsetBytes + (index * 4), value);
    }
  }

  public void Dispose() {

  }
}
