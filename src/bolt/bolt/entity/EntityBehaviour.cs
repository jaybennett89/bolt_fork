using Bolt;
using UE = UnityEngine;

namespace Bolt {
  /// <summary>
  /// Base class for unity behaviours that want to access Bolt methods
  /// </summary>
  /// <example>
  /// Inherit from Bolt.EntityBehaviour and attach the script to BoltEntity prefabs when you want to access Bolt methods for the entity.
  /// ```
  /// public class PlayerController : Bolt.EntityBehaviour
  /// {
  ///   public override void ControlGained()
  ///   {
  ///     GameCamera.instance.AddFollowTarget(this.transform);
  ///     MiniMap.instance.SetControlledPlayer(this.entity);
  ///   }
  /// }
  /// ```
  /// </example>
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
      set {
        _entity = value;
      }
    }

    /// <summary>
    /// Invoked when the entity has been initialized, before Attached
    /// </summary>
    /// <returns></returns>
    /// <example>
    /// Override when configuring an entity before the state is setup.
    /// ```
    /// public override void Initialized()
    /// {
    ///   MiniMap.instance.AddKnownPlayer(this.gameObject);
    /// }
    /// ```
    /// </example>
    public virtual void Initialized() { }

    /// <summary>
    /// Invoked when Bolt is aware of this entity and all internal state has been setup
    /// </summary>
    /// <returns></returns>
    /// <example>
    /// Override when configuring an entity and valid state is required.
    /// ```
    /// public override void Attached()
    /// {
    ///   state.AddCallback("name", NameChanged);
    ///   state.AddCallback("team", TeamChanged);
    /// }
    /// ```
    /// </example>
    public virtual void Attached() { }

    /// <summary>
    /// Invoked when this entity is removed from Bolt's awareness
    /// </summary>
    /// <returns></returns>
    /// <example>
    /// Override when action is required before the entity is detatched from the local game by Bolt.
    /// ```
    /// public override void Detached()
    /// {
    ///   MiniMap.instance.RemoveKnownPlayer(this.gameObject);
    /// {
    /// ``` 
    /// </example>
    public virtual void Detached() { }

    /// <summary>
    /// Invoked when Bolt is aware of this entity and all internal state has been setup
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <example>
    /// Override when configuring an entity and valid state is required.
    /// ```
    /// public override void Attached(IProtocolToken token)
    /// {
    ///   state.AddCallback("name", NameChanged);
    ///   state.AddCallback("team", TeamChanged);
    /// }
    /// ```
    /// </example>      
    public virtual void Attached(IProtocolToken token) { }

    /// <summary>
    /// Invoked when this entity is removed from Bolt's awareness
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <example>
    /// Override when action is required before the entity is detatched from the local game by Bolt.
    /// ```
    /// public override void Detached(IProtocolToken token)
    /// {
    ///   MiniMap.instance.RemoveKnownPlayer(this.gameObject);
    /// {
    /// ``` 
    /// </example>
    public virtual void Detached(IProtocolToken token) { }

    /// <summary>
    /// Invoked each simulation step on the owner
    /// </summary>
    /// <returns></returns>
    /// <example>
    /// Override when doing any state or entity updates that are authoritative and the method should only be called
    /// on the owner side.
    /// ```
    /// public override SimulateOwner()
    /// {
    ///   if(state.health < 100)
    ///   {
    ///     state.Modify().health += state.healthRegen * BoltNetwork.frameDeltaTime;
    ///   }
    /// }
    /// ```
    /// </example>
    public virtual void SimulateOwner() { }

    /// <summary>
    /// Invoked each simulation step on the controller
    /// </summary>
    /// <returns></returns>
    /// <example>
    /// Override to add inputs to the Bolt command loop when controlling an entity. One input command should be added to the queue
    /// per execution. Remember to create and compile a Command asset before using this method.
    /// ```
    /// bool forward;
    /// bool backward;
    /// bool left;
    /// bool right;
    /// 
    /// public override void SimulateController()
    /// {
    ///   IPlayerCommandInput input = PlayerCommand.Create();
    /// 
    ///   PollKeys();
    ///   
    ///   input.forward = forward;
    ///   input.backward = backward;
    ///   input.left = left;
    ///   input.right = right;
    ///   
    ///   entity.QueueInput(input);
    /// }
    /// ```
    /// </example>
    public virtual void SimulateController() { }

    /// <summary>
    /// Invoked when you gain control of this entity
    /// </summary>
    /// <returns></returns>
    /// <example>
    /// Override to recieve the callback when gaining control of the attached entity. 
    /// ```
    /// public override void ControlGained()
    /// {
    ///   GameCamera.instance.AddFollowTarget(this.transform);
    ///   MiniMap.instance.ControlGained(this.entity);
    /// }
    /// ```
    /// </example>
    public virtual void ControlGained() { }

    /// <summary>
    /// Invoked when you gain control of this entity
    /// </summary>
    /// <param name="token"></param> 
    /// <returns></returns>
    /// <example>
    /// Override to recieve the callback when gaining control of the attached entity. 
    /// ```
    /// public override void ControlGained(IProtocolToken token)
    /// {
    ///   GameCamera.instance.AddFollowTarget(this.transform);
    ///   MiniMap.instance.ControlGained(this.entity);
    /// }
    /// ```
    /// </example>
    public virtual void ControlGained(IProtocolToken token) { }

    /// <summary>
    /// Invoked when you lost control of this entity
    /// </summary>
    /// <returns></returns>
    /// <example>
    /// Override to recieve the callback when losing control of the attached entity. 
    /// ```
    /// public override void ControlLost(IProtocolToken token)
    /// {
    ///   GameCamera.instance.RemoveFollowTarget();
    ///   MiniMap.instance.ControlLost(this.entity);
    /// }
    /// ```
    /// </example>  
    public virtual void ControlLost() { }

    /// <summary>
    /// Invoked when you lost control of this entity
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <example>
    /// Override to recieve the callback when losing control of the attached entity. 
    /// ```
    /// public override void ControlLost()
    /// {
    ///   GameCamera.instance.RemoveFollowTarget();
    ///   MiniMap.instance.ControlLost(this.entity);
    /// }
    /// ```
    /// </example>  
    public virtual void ControlLost(IProtocolToken token) { }

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
    /// <returns></returns>
    /// <example>
    /// Override to execute inputs from the Bolt command loop when controlling an entity. On the client this method will be called multiple times per fixed frame,
    /// beginning with a reset to the last confirmed state and then once for each unverified input command in the queue. Remember to create and compile a Command 
    /// asset before using this method.
    /// 
    /// Use the cmd.isFirstExecution property to do any type of one-shot behaviour such as playing sound or animations. This will prevent it from being called each time
    /// the input is replayed on the client.
    /// ```
    /// public override ExecuteCommand(Bolt.Command command, bool resetState)
    /// {
    ///   if(resetState)
    ///   {
    ///     motor.SetState(cmd.Result.position);
    ///   }
    ///   else
    ///   {
    ///      cmd.Result.position = motor.Move(cmd.Input.forward, cmd.Input.backward, command.Input.left, command.Input.right);
    ///      
    ///      if (cmd.IsFirstExecution)
    ///      {
    ///         AnimatePlayer(cmd);
    ///      }
    ///   }
    /// }
    /// ```
    /// </example>
    public virtual void ExecuteCommand(Bolt.Command command, bool resetState) { }


  }

  /// <summary>
  /// Base class for unity behaviours that want to access Bolt methods with the state available also
  /// </summary>
  /// <typeparam name="TState">The type of state on this BoltEntity</typeparam>
  /// <example>
  /// Inherit scripts from Bolt.EntityBehaviour and attach them to BoltEntity prefabs where you want to access Bolt methods. Use the &ltTState&gt parameter
  /// to access the state of the entity also.
  /// 
  /// ```
  /// public class PlayerController : Bolt.EntityBehaviour&ltIPlayerState&gt
  /// {
  ///   public override void ControlGained()
  ///   {
  ///     state.AddCallback("team", TeamChanged);  
  ///     
  ///     GameCamera.instance.AddFollowTarget(this.transform);
  ///     MiniMap.instance.SetControlledPlayer(this.entity);
  ///   }
  ///   
  ///   void TeamChanged()
  ///   {
  ///     var nameplate = GetComponent&ltPlayerNameplate&gt();
  ///     if (state.team == 0) nameplate.color = Color.Blue;
  ///     else nameplate.color = Color.Red;
  ///   }
  /// }
  /// ```
  /// </example>
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
