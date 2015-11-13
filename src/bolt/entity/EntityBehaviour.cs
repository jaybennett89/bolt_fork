using System;
using Bolt;
using UE = UnityEngine;

namespace Bolt {
  /// <summary>
  /// Base class for unity behaviours that want to access Bolt methods
  /// </summary>
  /// <example>
  /// *Example:* Using ```Bolt.EntityBehaviour``` to write a simple PlayerController class. Attach to a valid bolt entity / prefab.
  /// 
  /// ```csharp
  /// public class PlayerController : Bolt.EntityBehaviour&ltIPlayerState&gt {
  /// 
  /// bool forward;
  /// bool backward;
  /// bool left;
  /// bool right;
  /// 
  /// public override void Initialized() {
  ///   MiniMap.instance.AddKnownPlayer(this.gameObject);
  /// }
  /// 
  /// public override void Attached() {
  ///   state.AddCallback("name", NameChanged);
  ///   state.AddCallback("team", TeamChanged);
  /// }
  /// 
  ///   public override void ControlGained() {
  ///     GameCamera.instance.AddFollowTarget(this.transform);
  ///     MiniMap.instance.SetControlledPlayer(this.entity);
  ///   }
  /// }
  /// public override SimulateOwner() {
  ///   if(state.health < 100)
  ///   {
  ///     state.Modify().health += state.healthRegen * BoltNetwork.frameDeltaTime;
  ///   }
  /// }
  /// 
  /// public override void SimulateController() {
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
  /// public override ExecuteCommand(Bolt.Command command, bool resetState) {
  ///   if(resetState) {
  ///     motor.SetState(cmd.Result.position);
  ///   }
  ///   else {
  ///      cmd.Result.position = motor.Move(cmd.Input.forward, cmd.Input.backward, command.Input.left, command.Input.right);
  ///      
  ///      if (cmd.IsFirstExecution) {
  ///         AnimatePlayer(cmd);
  ///      }
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
    /// Use the ```entity``` property to access the internal ```BoltEntity``` of the gameObject that this script is attached to.
    /// <example>
    /// *Example:* Passing the ```entity``` of this gameObject to a ```MiniMap```, giving it the position, facing direction and the 
    /// entity state (such as team, alive/dead, hostile, etc).
    /// 
    /// ```csharp
    /// public class PlayerController : Bolt.EntityBehaviour {
    ///   public override void ControlGained() {
    ///     GameCamera.instance.AddFollowTarget(this.transform);
    ///     MiniMap.instance.SetControlledPlayer(this.entity);
    ///   }
    /// }
    /// ```
    /// </example>
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

    Boolean IEntityBehaviour.invoke {
      get {
        return enabled;
      }
    }

    /// <summary>
    /// Invoked when the entity has been initialized, before Attached
    /// </summary>
    /// <example>
    /// *Example:* Notifying a ```MiniMap``` class to draw this gameObject by overriding the ```Initialized()``` method.
    /// 
    /// ```csharp
    /// public override void Initialized() {
    ///   MiniMap.instance.AddKnownPlayer(this.gameObject);
    /// }
    /// ```
    /// </example>
    public virtual void Initialized() { }

    /// <summary>
    /// Invoked when Bolt is aware of this entity and all internal state has been setup
    /// </summary>
    /// <example>
    /// *Example:* Overriding the ```Attached()``` method to add state change callbacks to the newly valid state.
    /// 
    /// ```csharp
    /// public override void Attached() {
    ///   state.AddCallback("name", NameChanged);
    ///   state.AddCallback("team", TeamChanged);
    /// }
    /// ```
    /// </example>
    public virtual void Attached() { }

    /// <summary>
    /// Invoked when this entity is removed from Bolt's awareness
    /// </summary>
    /// <example>
    /// *Example:* Notifying the game minimap to remove the entity upon detaching from the simulation.
    /// 
    /// ```csharp
    /// public override void Detached() {
    ///   MiniMap.instance.RemoveKnownPlayer(this.entity);
    /// {
    /// ``` 
    /// </example>
    public virtual void Detached() { }

    /// <summary>
    /// Invoked each simulation step on the owner
    /// </summary>
    /// <example>
    /// *Example:* Implementing an authoritative health regeneration update every 10th frame. Also fires the 
    /// ```DeathTrigger()``` on the state if health falls below zero.
    /// 
    /// ```csharp
    /// public override SimulateOwner() {
    ///   if(state.alive && state.Health.Current <= 0) {
    ///     using(var mod = state.Health.Modify()) {
    ///       mod.alive = false;
    ///       mod.DeathTrigger();
    ///     }
    ///   }
    ///   else if(state.alive && (BoltNetwork.frame % 10) == 0) {
    ///     using(var mod = state.Health.Modify()) {
    ///       mod.Current = Mathf.Clamp (state.Health.Current + (state.Health.RegenPer10 * BoltNetwork.frameDeltaTime),
    ///         // clamp from 0 to max health
    ///         0, state.Health.Max);
    ///     }
    ///   }
    /// }
    /// ```
    /// </example>
    public virtual void SimulateOwner() { }

    /// <summary>
    /// Invoked each simulation step on the controller
    /// </summary>
    /// <example>
    /// *Example:* Creating a simple WASD-style movement input command and adding it to the queue of inputs. One input command 
    /// should be added to the queue per execution and remember to create and compile a Command asset before using this method!
    /// 
    /// ```csharp
    /// bool forward;
    /// bool backward;
    /// bool left;
    /// bool right;
    /// 
    /// public override void SimulateController() {
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
    /// <example>
    /// *Example:* Using the ```ControlGained()``` callback to set up a ```GameCamera``` and ```MiniMap``` to focus on this entity.
    /// 
    /// ```csharp
    /// public override void ControlGained() {
    ///   GameCamera.instance.AddFollowTarget(this.transform);
    ///   MiniMap.instance.ControlGained(this.entity);
    /// }
    /// ``` 
    /// </example>
    public virtual void ControlGained() { }

    /// <summary>
    /// Invoked when you lost control of this entity
    /// </summary>
    /// <example>
    /// *Example:* Using the ```ControlLost()``` callback to remove the focus of a ```GameCamera``` and ```MiniMap```.
    /// 
    /// ```csharp
    /// public override void ControlLost() {
    ///   GameCamera.instance.RemoveFollowTarget();
    ///   MiniMap.instance.ControlLost(this.entity);
    /// }
    /// ```
    /// </example>  
    public virtual void ControlLost() { }

    /// <summary>
    /// Invoked on the owner when a remote connection is controlling this entity but we have not received any command for the current simulation frame.
    /// </summary>
    /// <param name="previous">The last valid command received</param>
    /// <example>
    /// *Example:* Handling missing input commands by using the last received input command to continue moving in the same direction.
    /// 
    /// ```csharp
    /// public override void MissingCommand(Bolt.Command previous)
    /// {
    ///   WASDCommand cmd = (WASDCommand)command;
    ///   
    ///   cmd.Result.position motor.Move(cmd.Input.forward, cmd.Input.backward, cmd.Input.left, cmd.Input.right);
    /// }
    /// ```
    /// </example>
    public virtual void MissingCommand(Bolt.Command previous) { }

    /// <summary>
    /// Invoked on both the owner and controller to execute a command
    /// </summary>
    /// <param name="command">The command to execute</param>
    /// <param name="resetState">Indicates if we should reset the state of the local motor or not</param>
    /// <example>
    /// *Example:* Executing a simple WASD movement command. On the client this method can be called multiple times per fixed frame,
    /// beginning with a reset to the last confirmed state (resetState == true), and then again for each unverified input command in the queue (resetState == false);
    /// 
    /// Use the cmd.isFirstExecution property to do any type of one-shot behaviour such as playing sound or animations. This will prevent it from being called each time
    /// the input is replayed on the client. 
    ///  
    /// Remember to create and compile a Command asset before using this method!
    /// 
    /// ```csharp
    /// public override ExecuteCommand(Bolt.Command command, bool resetState) {
    ///   WASDCommand cmd = (WASDCommand)command;
    ///   if(resetState) {
    ///     motor.SetState(cmd.Result.position);
    ///   }
    ///   else {
    ///      cmd.Result.position = motor.Move(cmd.Input.forward, cmd.Input.backward, cmd.Input.left, cmd.Input.right);
    ///      
    ///      if (cmd.IsFirstExecution) {
    ///         AnimatePlayer(cmd);
    ///      }
    ///   }
    /// }
    /// ```
    /// </example>
    public virtual void ExecuteCommand(Bolt.Command command, bool resetState) { }

    void IEntityBehaviour.Initialized() {
      throw new NotImplementedException();
    }

    void IEntityBehaviour.Attached() {
      throw new NotImplementedException();
    }

    void IEntityBehaviour.Detached() {
      throw new NotImplementedException();
    }

    void IEntityBehaviour.SimulateOwner() {
      throw new NotImplementedException();
    }

    void IEntityBehaviour.SimulateController() {
      throw new NotImplementedException();
    }

    void IEntityBehaviour.ControlLost() {
      throw new NotImplementedException();
    }

    void IEntityBehaviour.ControlGained() {
      throw new NotImplementedException();
    }

    void IEntityBehaviour.MissingCommand(Command previous) {
      throw new NotImplementedException();
    }

    void IEntityBehaviour.ExecuteCommand(Command command, Boolean resetState) {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// Base class for unity behaviours that want to access Bolt methods with the state available also
  /// </summary>
  /// <typeparam name="TState">The type of state on this BoltEntity</typeparam>
  /// <example>
  /// *Example:* Using the ```IPlayerState``` type as a parameter and using its property ```state.team``` in code.
  /// 
  /// ```csharp
  /// public class PlayerController : Bolt.EntityBehaviour&ltIPlayerState&gt {
  ///   public override void ControlGained() {
  ///     state.AddCallback("team", TeamChanged);  
  ///   }
  ///   
  ///   void TeamChanged() {
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
    /// <example>
    /// *Example:* Using the ```state``` property to set up state callbacks.
    /// 
    /// ```csharp
    /// public class PlayerController : Bolt.EntityBehaviour&ltIPlayerState&gt {
    ///   public override void ControlGained() {
    ///     state.AddCallback("team", TeamChanged);  
    ///   }
    ///   
    ///   void TeamChanged() {
    ///     var nameplate = GetComponent&ltPlayerNameplate&gt();
    ///     if (state.team == 0) nameplate.color = Color.Blue;
    ///     else nameplate.color = Color.Red;
    ///   }
    /// }
    /// ```
    /// </example>
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
