﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  /// <summary>
  /// Interface for unity behaviours that want to access Bolt methods
  /// </summary>
  public interface IEntityBehaviour {
    bool invoke { get; }
    BoltEntity entity { get; set; }
    void Initialized();

    void Attached();
    void Detached();

    void SimulateOwner();
    void SimulateController();

    void ControlLost();
    void ControlGained();

    void MissingCommand(Bolt.Command previous);
    void ExecuteCommand(Bolt.Command command, bool resetState);
  }

  public interface IEntityReplicationFilter {
    bool AllowReplicationTo(BoltConnection connection);
  }

  /// <summary>
  /// Interface which can be implemented on a behaviour attached to an entity which lets you provide
  /// custom priority calculations for state and events.
  /// </summary>
  public interface IPriorityCalculator {
    /// <summary>
    /// Called for calculating the priority of this entity for the connection passed in
    /// </summary>
    /// <param name="connection">The connection we are calculating priority for</param>
    /// <param name="mask">The mask of properties with updated values we want to replicate</param>
    /// <param name="skipped">How many packets since we sent an update for this entity</param>
    /// <returns>The priority of the entity</returns>
    float CalculateStatePriority(BoltConnection connection, int skipped);

    /// <summary>
    /// Called for calculating the priority of an event sent to this entity for the connection passed in
    /// </summary>
    /// <param name="connection">The connection we are calculating priority for</param>
    /// <param name="evnt">The event we are calculating priority for</param>
    /// <returns>The priority of the event</returns>
    float CalculateEventPriority(BoltConnection connection, Event evnt);

    bool Always { get; }
  }

  /// <summary>
  /// Interface for unity behaviours that want to access Bolt methods
  /// </summary>
  /// <typeparam name="TState">Bolt state of the entity</typeparam>
  public interface IEntityBehaviour<TState> : IEntityBehaviour where TState : IState {
    TState state { get; }
  }

  internal interface IEntitySerializer {
    TypeId TypeId { get; }

    void OnRender();
    void OnInitialized();
    void OnCreated(Entity entity);
    void OnParentChanging(Entity newParent, Entity oldParent);

    void OnSimulateBefore();
    void OnSimulateAfter();

    void OnControlGained();
    void OnControlLost();

    BitSet GetDefaultMask();
    BitSet GetFilter(BoltConnection connection, EntityProxy proxy);

    void DebugInfo();
    void InitProxy(EntityProxy p);

    int Pack(BoltConnection connection, UdpPacket stream, EntityProxyEnvelope proxy);
    void Read(BoltConnection connection, UdpPacket stream, int frame);
  }

  internal interface IEntitySerializer<TState> : IEntitySerializer where TState : IState {
    TState state { get; }
  }
}
