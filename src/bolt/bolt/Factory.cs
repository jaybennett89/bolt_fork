using Bolt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  interface IFactory {
    Type TypeObject { get; }
    TypeId TypeId { get; }
    //Guid TypeGuid { get; }
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
    static Dictionary<Guid, IFactory> _factoriesByGuid = new Dictionary<Guid, IFactory>();
    static Dictionary<TypeId, IFactory> _factoriesByTypeId = new Dictionary<TypeId, IFactory>();

    internal static bool IsEmpty {
      get { return _factoriesByTypeId.Count == 0; }
    }

    internal static void Register(Bolt.IFactory factory) {
      //_factoriesByGuid.Add(factory.TypeGuid, factory);
      _factoriesByTypeId.Add(factory.TypeId, factory);
    }

    internal static Bolt.IFactory GetFactory(TypeId id) {
      return _factoriesByTypeId[id];
    }

    internal static Bolt.IFactory GetFactory(Guid id) {
      return _factoriesByGuid[id];
    }

    internal static IEventFactory GetEventFactory(TypeId id) {
      return (IEventFactory)_factoriesByTypeId[id];
    }

    internal static IEventFactory GetEventFactory(Guid id) {
      return (IEventFactory)_factoriesByGuid[id];
    }

    internal static Event NewEvent(TypeId id) {
      Event ev;

      ev = (Event)Create(id);
      ev.IncrementRefs();

      return ev;
    }

    internal static Event NewEvent(Guid id) {
      Event ev;

      ev = (Event)Create(id);
      ev.IncrementRefs();

      return ev;
    }

    internal static Command NewCommand(TypeId id) {
      return (Command)Create(id);
    }


    internal static Command NewCommand(Guid id) {
      return (Command)Create(id);
    }

    internal static IEntitySerializer NewSerializer(Bolt.TypeId id) {
      return (IEntitySerializer)Create(id);
    }

    internal static IEntitySerializer NewSerializer(Guid guid) {
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

    static object Create(Guid id) {
#if DEBUG
      if (_factoriesByGuid.ContainsKey(id) == false) {
        BoltLog.Error("Unknown [Guid {0}]", id);
      }
#endif

      return _factoriesByGuid[id].Create();
    }

    internal static void UnregisterAll() {
      _factoriesByGuid.Clear();
      _factoriesByTypeId.Clear();

      _factoriesByGuid = new Dictionary<Guid, IFactory>(128);
      _factoriesByTypeId = new Dictionary<TypeId, IFactory>(128, TypeId.EqualityComparer.Instance);
    }

  }

}