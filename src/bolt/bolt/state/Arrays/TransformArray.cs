using Bolt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Represents an array of transforms on a state
/// </summary>
[Documentation]
public class TransformArray {
  internal Bolt.State state;

  internal int length;
  internal int offsetObjects;
  internal int offsetStorage;

  internal TransformArray(Bolt.State state, int offsetStorage, int offsetObjects, int length) {
    this.state = state;
    this.length = length;
    this.offsetStorage = offsetStorage;
    this.offsetObjects = offsetObjects;
  }

  public int Length {
    get {
      return length;
    }
  }

  public Bolt.TransformData this[int index] {
    get {
      if (index < 0 || index >= length) {
        throw new IndexOutOfRangeException();
      }

      return (Bolt.TransformData)state.Objects[offsetObjects + index];
    }
    set {
      if (index < 0 || index >= length) {
        throw new IndexOutOfRangeException();
      }

      Assert.True(
        (state.Objects[offsetObjects + index] == null)
        ||
        (state.Objects[offsetObjects + index] is Bolt.TransformData)
      );

      state.Objects[offsetObjects + index] = value;
    }
  }
}
