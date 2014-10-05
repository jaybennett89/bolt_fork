using Bolt;
using System;
using System.Collections.Generic;

internal static class BoltFactory {
  static Dictionary<Type, IBoltEventFactory> _eventFactoryByType = new Dictionary<Type, IBoltEventFactory>();
  static Dictionary<ushort, IBoltEventFactory> _eventFactoryById = new Dictionary<ushort, IBoltEventFactory>();
  static Dictionary<TypeId, ICommandFactory> _cmdFactoryById = new Dictionary<TypeId, ICommandFactory>();
  static Dictionary<TypeId, IEntitySerializerFactory> _serializerFactoryById = new Dictionary<TypeId, Bolt.IEntitySerializerFactory>(TypeId.EqualityComparer.Instance);

  internal static bool IsEmpty {
    get { return _serializerFactoryById.Count == 0 && _cmdFactoryById.Count == 0; }
  }

  internal static void Register(Bolt.IEntitySerializerFactory factory) {
    _serializerFactoryById.Add(factory.TypeId, factory);
  }

  internal static void Register(ICommandFactory factory) {
    _cmdFactoryById.Add(factory.TypeId, factory);
  }

  internal static void Register(IBoltEventFactory factory) {
    if (factory.eventId != ushort.MaxValue) {
      _eventFactoryById.Add(factory.eventId, factory);
    }

    _eventFactoryByType.Add(factory.eventType, factory);
  }

  internal static IBoltEventFactory GetEventFactory(ushort id) {
    return _eventFactoryById[id];
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

  internal static IBoltEvent NewEvent(Type t) {
    IBoltEventFactory f;

    if (_eventFactoryByType.TryGetValue(t, out f)) {
      BoltEventBase evnt = (BoltEventBase)f.Create();
      evnt.RefCountIncrement();

      return (IBoltEvent)(object)evnt;
    }

    throw new BoltException("unknown event type {0}", t.FullName);
  }

  internal static T NewEvent<T>() where T : IBoltEvent {
    return (T)(object)NewEvent(typeof(T));
  }

  internal static Bolt.Command NewCommand(TypeId id) {
    return _cmdFactoryById[id].Create();
  }

  internal static Bolt.IEntitySerializer NewSerializer(Bolt.TypeId id) {
    return _serializerFactoryById[id].Create();
  }

  internal static void UnregisterAll() {
    _eventFactoryByType.Clear();
    _eventFactoryByType = new Dictionary<Type, IBoltEventFactory>(128);

    _eventFactoryById.Clear();
    _eventFactoryById = new Dictionary<ushort, IBoltEventFactory>(128);

    _cmdFactoryById.Clear();
    _cmdFactoryById = new Dictionary<Bolt.TypeId, Bolt.ICommandFactory>(128, TypeId.EqualityComparer.Instance);

    _serializerFactoryById.Clear();
    _serializerFactoryById = new Dictionary<TypeId, Bolt.IEntitySerializerFactory>(128, TypeId.EqualityComparer.Instance);
  }

}
