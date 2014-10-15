using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Represents an array of entities on a state
/// </summary>
public struct EntityArray {
  internal Bolt.State.Frame frame;
  internal int offsetObjects;
  internal int offsetBytes;
  internal int length;

  internal EntityArray(Bolt.State.Frame frame, int offsetBytes, int offsetObjects, int length) {
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

  public BoltEntity this[int index] {
    get {
      if (index < 0 || index >= length) {
        throw new IndexOutOfRangeException();
      }

      Bolt.InstanceId id = new Bolt.InstanceId(Bolt.Blit.ReadI32(frame.Data, offsetBytes + (index * 4)));
      Bolt.Entity entity = BoltCore.FindEntity(id);

      if (entity) {
        return entity.UnityObject;
      }

      return null;
    }
  }

  /// <summary>
  /// Creates aa modifier object for this array
  /// </summary>
  /// <returns>The modifier object</returns>
  public EntityArrayModifier Modify() {
    return new EntityArrayModifier(this);
  }
}

/// <summary>
/// Object which allows you to modify an entity array
/// </summary>
public class EntityArrayModifier : IDisposable {
  EntityArray array;

  internal EntityArrayModifier(EntityArray array) {
    this.array = array;
  }

  /// <summary>
  /// How many entities are available in this array
  /// </summary>
  public int Length {
    get {
      return array.length;
    }
  }

  public BoltEntity this[int index] {
    get { return array[index]; }
    set {
      if (index < 0 || index >= array.length) {
        throw new IndexOutOfRangeException();
      }

      if (value) {
        if (value.isAttached) {
          Bolt.Blit.PackI32(array.frame.Data, array.offsetBytes + (index * 4), value._entity.InstanceId.Value);
        }
        else {
          BoltLog.Error("You can't put an entity which is not attached into the array");
        }
      }
      else {
        Bolt.Blit.PackI32(array.frame.Data, array.offsetBytes + (index * 4), 0);
      }
    }
  }

  public void Dispose() {

  }
}
