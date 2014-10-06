using Bolt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  interface IFactory {
    Type TypeObject { get; }
    TypeId TypeId { get; }
    object Create();
  }

  interface IEventFactory : IFactory {
    void Dispatch(Event ev, object target);
  }

  static class Factory {
    static Dictionary<TypeId, IFactory> _factories = new Dictionary<TypeId, IFactory>();

    internal static bool IsEmpty {
      get { return _factories.Count == 0; }
    }

    internal static void Register(Bolt.IFactory factory) {
      _factories.Add(factory.TypeId, factory);
    }

    internal static IEventFactory GetEventFactory(TypeId id) {
      return (IEventFactory)_factories[id];
    }

    internal static Event NewEvent(TypeId id) {
      Event ev;

      ev = (Event)_factories[id].Create();
      ev.IncrementRefs();

      return ev;
    }

    internal static Command NewCommand(TypeId id) {
      return (Command)_factories[id].Create();
    }

    internal static IEntitySerializer NewSerializer(Bolt.TypeId id) {
      return (IEntitySerializer)_factories[id].Create();
    }

    internal static void UnregisterAll() {
      _factories.Clear();
      _factories = new Dictionary<TypeId, IFactory>(128, TypeId.EqualityComparer.Instance);
    }
  }

}