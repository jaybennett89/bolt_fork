using UE = UnityEngine;

namespace Bolt {
  public abstract class EntityBehaviour : UE.MonoBehaviour, IEntityBehaviour {
    internal BoltEntity _entity;

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

    public virtual void Initialized() { }
    public virtual void Attached() { }
    public virtual void Detached() { }
    public virtual void SimulateOwner() { }
    public virtual void SimulateController() { }
    public virtual void ControlGained() { }
    public virtual void ControlLost() { }
    public virtual void MissingCommand(Bolt.Command previous) { }
    public virtual void ExecuteCommand(Bolt.Command command, bool resetState) { }
  }

  public abstract class EntityBehaviour<TState> : EntityBehaviour {
    public TState state {
      get { return entity.GetState<TState>(); }
    }
  }

}

namespace BoltInternal {
  public abstract class EntityEventListenerBase : Bolt.EntityBehaviour {
    public sealed override void Initialized() {
      entity.Entity.AddEventListener(this);
    }
  }

  public abstract class EntityEventListenerBase<TState> : EntityEventListenerBase {
    public TState state {
      get { return entity.GetState<TState>(); }
    }
  }
}
