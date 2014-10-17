using Bolt;
using UE = UnityEngine;

namespace Bolt {
  /// <summary>
  /// Base class for unity behaviours that want to access Bolt methods
  /// </summary>
  [Documentation]
  public abstract class EntityBehaviour : UE.MonoBehaviour, IEntityBehaviour {
    internal BoltEntity _entity;

    /// <summary>
    /// The entity for this behaviour
    /// </summary>
    public BoltEntity entity {
      get {
        if (!_entity) {
          UE.Transform t = transform;

          while (t && !_entity) {
            _entity = t.GetComponent<BoltEntity>();
            t = t.parent;
          }

          if (!_entity) {
            BoltLog.Error("Could not find a Bolt Entity component attached to '{0}' or any of its parents", gameObject.name);
          }
        }

        return _entity;
      }
    }

    /// <summary>
    /// Invoked when the entity has been initialized, before Attached
    /// </summary>
    public virtual void Initialized() { }

    /// <summary>
    /// Invoked when Bolt is aware of this entity and all internal state has been setup
    /// </summary>
    public virtual void Attached() { }

    /// <summary>
    /// Invoked when this entity is removed from Bolts awareness
    /// </summary>
    public virtual void Detached() { }

    /// <summary>
    /// Invoked each simulation step on the owner
    /// </summary>
    public virtual void SimulateOwner() { }

    /// <summary>
    /// Invoked each simulation step on the controller
    /// </summary>
    public virtual void SimulateController() { }

    /// <summary>
    /// Invoked when you gain control of this entity
    /// </summary>
    public virtual void ControlGained() { }

    /// <summary>
    /// Invoked when you lost control of this entity
    /// </summary>
    public virtual void ControlLost() { }

    /// <summary>
    /// Invoked on the owner when a remote connection is controlling this entity but we have not received any command for the current simulation frame.
    /// </summary>
    /// <param name="previous"></param>
    public virtual void MissingCommand(Bolt.Command previous) { }

    /// <summary>
    /// Invoked on both the owner and controller to execute a command
    /// </summary>
    /// <param name="command">The command to execute</param>
    /// <param name="resetState">Indicates if we should reset the state of the local motor or not</param>
    public virtual void ExecuteCommand(Bolt.Command command, bool resetState) { }
  }

  /// <summary>
  /// Base class for unity behaviours that want to access Bolt methods with the state available also
  /// </summary>
  /// <typeparam name="TState"></typeparam>
  [Documentation(Alias = "Bolt.EntityBehaviour<TState>")]
  public abstract class EntityBehaviour<TState> : EntityBehaviour {
    /// <summary>
    /// The state for this behaviours entity
    /// </summary>
    public TState state {
      get { return entity.GetState<TState>(); }
    }
  }
}

namespace BoltInternal {
  [Documentation(Alias = "Bolt.EntityEventListener")]
  public abstract class EntityEventListenerBase : Bolt.EntityBehaviour {
    public sealed override void Initialized() {
      entity.Entity.AddEventListener(this);
    }
  }

  [Documentation(Alias = "Bolt.EntityEventListener<TState>")]
  public abstract class EntityEventListenerBase<TState> : EntityEventListenerBase {
    public TState state {
      get { return entity.GetState<TState>(); }
    }
  }
}
