using Bolt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  interface IFactory {
    Type TypeObject { get; }
    TypeId TypeId { get; }
    UniqueId TypeUniqueId { get; }
    object Create();
  }

  interface IEventFactory : IFactory {
    void Dispatch(Event ev, object target);
  }

  interface ISerializerFactory : IFactory {
  }

  interface ICommandFactory : IFactory {
  }

  static class Factory {
    static Dictionary<UniqueId, IFactory> _factoriesByUniqueId = new Dictionary<UniqueId, IFactory>();
    static Dictionary<TypeId, IFactory> _factoriesByTypeId = new Dictionary<TypeId, IFactory>();

    internal static bool IsEmpty {
      get { return _factoriesByTypeId.Count == 0; }
    }

    internal static void Register(Bolt.IFactory factory) {
      _factoriesByUniqueId.Add(factory.TypeUniqueId, factory);
      _factoriesByTypeId.Add(factory.TypeId, factory);
    }

    internal static Bolt.IFactory GetFactory(TypeId id) {
      BoltLog.Debug("Looking up factory {0}", id);
      return _factoriesByTypeId[id];
    }

    internal static Bolt.IFactory GetFactory(UniqueId id) {
      BoltLog.Debug("Looking up factory {0}", id);
      return _factoriesByUniqueId[id];
    }

    internal static IEventFactory GetEventFactory(TypeId id) {
      return (IEventFactory)_factoriesByTypeId[id];
    }

    internal static IEventFactory GetEventFactory(UniqueId id) {
      return (IEventFactory)_factoriesByUniqueId[id];
    }

    internal static Event NewEvent(TypeId id) {
      Event ev;

      ev = (Event)Create(id);
      ev.IncrementRefs();

      return ev;
    }

    internal static Event NewEvent(UniqueId id) {
      Event ev;

      ev = (Event)Create(id);
      ev.IncrementRefs();

      return ev;
    }

    internal static Command NewCommand(TypeId id) {
      return (Command)Create(id);
    }


    internal static Command NewCommand(UniqueId id) {
      return (Command)Create(id);
    }

    internal static IEntitySerializer NewSerializer(Bolt.TypeId id) {
      return (IEntitySerializer)Create(id);
    }

    internal static IEntitySerializer NewSerializer(UniqueId guid) {
      return (IEntitySerializer)Create(guid);
    }

    static object Create(TypeId id) {
#if DEBUG
      if (_factoriesByTypeId.ContainsKey(id) == false) {
        BoltLog.Error("Unknown {0}", id);
      }
#endif

      return _factoriesByTypeId[id].Create();
    }

    static object Create(UniqueId id) {
#if DEBUG
      if (_factoriesByUniqueId.ContainsKey(id) == false) {
        BoltLog.Error("Unknown {0}", id);
      }
#endif

      return _factoriesByUniqueId[id].Create();
    }

    internal static void UnregisterAll() {
      _factoriesByTypeId.Clear();
      _factoriesByUniqueId.Clear();

      _factoriesByTypeId = new Dictionary<TypeId, IFactory>(128, TypeId.EqualityComparer.Instance);
      _factoriesByUniqueId = new Dictionary<UniqueId, IFactory>(128, UniqueId.EqualityComparer.Instance);
    }

  }

}