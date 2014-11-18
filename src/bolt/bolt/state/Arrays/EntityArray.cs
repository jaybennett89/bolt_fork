﻿using Bolt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

/// <summary>
/// Represents an array of entities on a state
/// </summary>
[Documentation]
public struct EntityArray {
  internal Bolt.State.NetworkFrame frame;
  internal int offsetObjects;
  internal int offsetBytes;
  internal int length;

  internal EntityArray(Bolt.State.NetworkFrame frame, int offsetBytes, int offsetObjects, int length) {
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

      Bolt.NetworkId id = Bolt.Blit.ReadNetworkId(frame.Data, offsetBytes + (index * 8));
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
[Documentation]
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
          Bolt.Blit.PackNetworkId(array.frame.Data, array.offsetBytes + (index * 8), value._entity.NetworkId);
        }
        else {
          BoltLog.Error("You can't put an entity which is not attached into the array");
        }
      }
      else {
        Bolt.Blit.PackNetworkId(array.frame.Data, array.offsetBytes + (index * 8), default(NetworkId));
      }
    }
  }

  public void Dispose() {

  }
}
