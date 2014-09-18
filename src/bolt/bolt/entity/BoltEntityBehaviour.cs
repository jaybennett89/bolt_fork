using System;
using UnityEngine;

/// <summary>
/// Base class for all entity behaviours
/// </summary>
public abstract class BoltEntityBehaviourBase : MonoBehaviour {
  internal BoltEntity _entity;

  public BoltEntity entity {
    get {
      if (!_entity) {
        Transform t = transform;

        while (t && !_entity) {
          _entity = t.GetComponent<BoltEntity>();
          t = t.parent;
        }

        if (!_entity) {
          BoltLog.Error("could not find entity attached to '{0}' or any of it's parents", gameObject.name);
        }
      }

      return _entity;
    }
  }


  /// <summary>
  /// The entity this behaviour attached to
  /// </summary>
  [Obsolete("Use BoltEntityBehaviour.entity instead")]
  public BoltEntity boltEntity {
    get { return entity; }
  }

  public BoltEntitySerializer serializer {
    get {
      if (entity) {
        return entity.boltSerializer;
      }

      return null;
    }
  }

  /// <summary>
  /// The serializer for the entity this behaviour is attached to
  /// </summary>
  [Obsolete("Use BoltEntityBehaviour.serializer instead")]
  public BoltEntitySerializer boltSerializer {
    get { return serializer; }
  }

  /// <summary>
  /// The frame interpolator for the entity this behaviour is attached to
  /// </summary>
  [Obsolete("This property is being removed and all it's features will be rolled into a future version of Bolt")]
  public BoltFrameInterpolator boltFrameInterpolator {
    get {
      if (entity) {
        return entity.GetComponent<BoltFrameInterpolator>();
      }

      return null;
    }
  }


  protected void OnEnable () {
    entity._eventDispatcher.Register(this);
  }

  protected void OnDisable () {
    entity._eventDispatcher.Remove(this);
  }

  /// <summary>
  /// Called when the entity is attached
  /// </summary>
  public virtual void Attached () { }

  /// <summary>
  /// Called when the entity is detached
  /// </summary>
  public virtual void Detached () { }

  /// <summary>
  /// Called for simulating one frame the owner
  /// </summary>
  public virtual void SimulateOwner () { }

  /// <summary>
  /// Called for simulating one frame a proxy
  /// </summary>
  public virtual void SimulateProxy () { }

  /// <summary>
  /// Called for simulating one frame on the controller
  /// </summary>
  public virtual void SimulateController () { }

  public virtual void ControlGained () { }
  public virtual void ControlLost () { }

  /// <summary>
  /// Called for executing one command on the owner and controller
  /// </summary>
  /// <param name="cmd">The command we are executing</param>
  /// <param name="resetState">If we should reset the internal state to the state of this command instead of executing the input</param>
  public virtual void ExecuteCommand (BoltCommand cmd, bool resetState) { }
}
