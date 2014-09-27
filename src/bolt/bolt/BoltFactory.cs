using Bolt;
using System;
using System.Collections.Generic;

/// <summary>
/// Responsible for creating events and commands during runtime
/// </summary>
public static class BoltFactory {
  static Dictionary<Type, IBoltStateFactory> _stateFactoryByType = new Dictionary<Type, IBoltStateFactory>();

  static Dictionary<Type, Bolt.IEntitySerializerFactory> _state2FactoryByType = new Dictionary<Type, Bolt.IEntitySerializerFactory>();
  static Dictionary<TypeId, Bolt.IEntitySerializerFactory> _state2FactoryById = new Dictionary<TypeId, Bolt.IEntitySerializerFactory>(TypeId.EqualityComparer.Instance);

  static Dictionary<Type, IBoltEventFactory> _eventFactoryByType = new Dictionary<Type, IBoltEventFactory>();
  static Dictionary<ushort, IBoltEventFactory> _eventFactoryById = new Dictionary<ushort, IBoltEventFactory>();
  static Dictionary<Type, IBoltCommandFactory> _cmdFactoryByType = new Dictionary<Type, IBoltCommandFactory>();
  static Dictionary<ushort, IBoltCommandFactory> _cmdFactoryById = new Dictionary<ushort, IBoltCommandFactory>();

  public static bool IsEmpty {
    get {
      return
        _state2FactoryByType.Count == 0 &&
        _state2FactoryById.Count == 0 &&
        _stateFactoryByType.Count == 0 &&
        _eventFactoryByType.Count == 0 &&
        _eventFactoryById.Count == 0 &&
        _cmdFactoryByType.Count == 0 &&
        _cmdFactoryById.Count == 0;
    }
  }

  /// <summary>
  /// Creates a new event of type T, which has to be an interface
  /// which inherits from IBoltEvent
  /// </summary>
  /// <typeparam name="T">The type of event to create</typeparam>
  /// <example>
  /// IUserEventType evt = BoltFactory.NewEvent&lt;IUserEventType&gt;();
  /// </example>
  public static T NewEvent<T>() where T : IBoltEvent {
    return (T)(object)NewEvent(typeof(T));
  }

  /// <summary>
  /// Creates a new command of type T, which has to be a class
  /// which inherits from BoltCommand
  /// </summary>
  /// <typeparam name="T">The type of command to create</typeparam>
  /// <example>
  /// UserCommandType cmd = BoltFactory.NewCommand&lt;UserCommandType&gt;();
  /// </example>
  public static T NewCommand<T>() where T : BoltCommand {
    return (T)NewCommand(typeof(T));
  }

  internal static T NewState<T>() where T : IBoltState {
    return (T)NewState(typeof(T));
  }

  /// <summary>
  /// Creates a new event of the type passed as the argument, which has to be an interface
  /// which inherits from IBoltEvent
  /// </summary>
  /// <example>
  /// IBoltEvent evt = BoltFactory.NewEvent(typeof(IUserEventType));
  /// </example>
  public static IBoltEvent NewEvent(Type t) {
    IBoltEventFactory f;

    if (_eventFactoryByType.TryGetValue(t, out f)) {
      BoltEventBase evnt = (BoltEventBase)f.Create();
      evnt.RefCountIncrement();

      return (IBoltEvent)(object)evnt;
    }

    throw new BoltException("unknown event type {0}", t.FullName);
  }

  /// <summary>
  /// Creates a new command of the type passed as the argument, which has to be a class
  /// which inherits from BoltCommand
  /// </summary>
  /// <example>
  /// BoltCommand cmd = BoltFactory.NewCommand(typeof(UserCommandType));
  /// </example>
  public static BoltCommand NewCommand(Type t) {
    IBoltCommandFactory f;

    if (_cmdFactoryByType.TryGetValue(t, out f)) {
      return f.Create();
    }

    throw new BoltException("unknown command type {0}", t);
  }

  internal static IBoltState NewState(Type t) {
    IBoltStateFactory f;

    if (_stateFactoryByType.TryGetValue(t, out f)) {
      return f.Create();
    }

    throw new BoltException("unknown state type {0}", t);
  }


  internal static void Register(IBoltStateFactory factory) {
    _stateFactoryByType.Add(factory.stateType, factory);
  }

  internal static void Register(Bolt.IEntitySerializerFactory factory) {
    _state2FactoryById.Add(factory.TypeId, factory);
    _state2FactoryByType.Add(factory.TypeObject, factory);
  }

  /// <summary>
  /// Registers a new command factory, this has to be done in the BoltCallback.IStartDone callback. This allows
  /// you to register custom command types which are not compiled through a command asset.
  /// </summary>
  /// <example>
  ///using UnityEngine;
  /// 
  ///public class ExampleCallbacks : BoltCallback, BoltCallback.IStartDone {
  ///	void BoltCallback.IStartDone.Invoke () {
  ///		BoltFactory.Register(new UserCommandTypeFactory());
  ///	}
  ///}
  /// </example>
  public static void Register(IBoltCommandFactory factory) {
    _cmdFactoryById.Add(factory.commandId, factory);
    _cmdFactoryByType.Add(factory.commandType, factory);
  }

  /// <summary>
  /// Registers a new event factory, this has to be done in the BoltCallback.IStartDone callback. This allows
  /// you to register custom event types which are not compiled through an event asset.
  /// </summary>
  /// <example>
  ///using UnityEngine;
  /// 
  ///public class ExampleCallbacks : BoltCallback, BoltCallback.IStartDone {
  ///	void BoltCallback.IStartDone.Invoke () {
  ///		BoltFactory.Register(new UserEventTypeFactory());
  ///	}
  ///}
  /// </example>
  public static void Register(IBoltEventFactory factory) {
    if (factory.eventId != ushort.MaxValue) {
      _eventFactoryById.Add(factory.eventId, factory);
    }

    _eventFactoryByType.Add(factory.eventType, factory);
  }

  internal static BoltCommand NewCommand(ushort id) {
    IBoltCommandFactory handler;

    if (_cmdFactoryById.TryGetValue(id, out handler)) {
      return handler.Create();
    }

    throw new BoltException("unknown command id {0}", id);
  }

  internal static BoltEventBase NewEvent(ushort id) {
    IBoltEventFactory handler;

    if (_eventFactoryById.TryGetValue(id, out handler)) {
      BoltEventBase evnt = (BoltEventBase)handler.Create();
      evnt.RefCountIncrement();
      return evnt;
    }

    throw new BoltException("unknown event id {0}", id);
  }

  internal static IBoltEventFactory GetEventFactory(ushort id) {
    return _eventFactoryById[id];
  }

  internal static IBoltCommandFactory GetCommandFactory(ushort id) {
    return _cmdFactoryById[id];
  }

  internal static Bolt.IEntitySerializer CreateSerializer(Bolt.TypeId id) {
    return _state2FactoryById[id].Create();
  }

  internal static void UnregisterAll() {
    _eventFactoryByType.Clear();
    _eventFactoryByType = new Dictionary<Type, IBoltEventFactory>(128);

    _eventFactoryById.Clear();
    _eventFactoryById = new Dictionary<ushort, IBoltEventFactory>(128);

    _cmdFactoryByType.Clear();
    _cmdFactoryByType = new Dictionary<Type, IBoltCommandFactory>(128);

    _cmdFactoryById.Clear();
    _cmdFactoryById = new Dictionary<ushort, IBoltCommandFactory>(128);

    _stateFactoryByType.Clear();
    _stateFactoryByType = new Dictionary<Type, IBoltStateFactory>(128);

    _state2FactoryByType.Clear();
    _state2FactoryByType = new Dictionary<Type, Bolt.IEntitySerializerFactory>(128);

    _state2FactoryById.Clear();
    _state2FactoryById = new Dictionary<TypeId, Bolt.IEntitySerializerFactory>(128, TypeId.EqualityComparer.Instance);
  }

}
