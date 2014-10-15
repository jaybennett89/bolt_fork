using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Represents an array of integers on a state
/// </summary>
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

  /// <summary>
  /// The size of the array
  /// </summary>
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

  /// <summary>
  /// Creates aa modifier object for this array
  /// </summary>
  /// <returns>The modifier object</returns>
  public IntegerArrayModifier Modify() {
    return new IntegerArrayModifier(this);
  }
}

/// <summary>
/// Object which allows you to modify an integer array
/// </summary>
public class IntegerArrayModifier : IDisposable {
  IntegerArray array;

  internal IntegerArrayModifier(IntegerArray array) {
    this.array = array;
  }

  /// <summary>
  /// The size of the array
  /// </summary>
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
