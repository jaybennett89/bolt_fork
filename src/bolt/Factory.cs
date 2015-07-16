using System;
using System.Collections.Generic;

namespace Bolt {
  interface IFactory {
    Type TypeObject { get; }
    TypeId TypeId { get; }
    UniqueId TypeKey { get; }
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
    static Dictionary<byte, Type> _id2token = new Dictionary<byte, Type>();
    static Dictionary<Type, byte> _token2id = new Dictionary<Type, byte>();

    static Dictionary<Type, IFactory> _factoriesByType = new Dictionary<Type, IFactory>();
    static Dictionary<TypeId, IFactory> _factoriesById = new Dictionary<TypeId, IFactory>();
    static Dictionary<UniqueId, IFactory> _factoriesByKey = new Dictionary<UniqueId, IFactory>();

    internal static bool IsEmpty {
      get { return _factoriesById.Count == 0; }
    }

    internal static void Register(IFactory factory) {
      _factoriesById.Add(factory.TypeId, factory);
      _factoriesByKey.Add(factory.TypeKey, factory);
      _factoriesByType.Add(factory.TypeObject, factory);
    }

    internal static IFactory GetFactory(TypeId id) {
#if DEBUG
      if (!_factoriesById.ContainsKey(id)) {
        BoltLog.Error("Unknown factory {0}", id);
        return null;
      }
#endif

      return _factoriesById[id];
    }

    internal static IFactory GetFactory(UniqueId id) {
#if DEBUG
      if (!_factoriesByKey.ContainsKey(id)) {
        BoltLog.Error("Unknown factory {0}", id);
        return null;
      }
#endif

      return _factoriesByKey[id];
    }

    internal static IEventFactory GetEventFactory(TypeId id) {
      return (IEventFactory)_factoriesById[id];
    }

    internal static IEventFactory GetEventFactory(UniqueId id) {
      return (IEventFactory)_factoriesByKey[id];
    }

    internal static Event NewEvent(TypeId id) {
      Event ev;

      ev = (Event)Create(id);

      return ev;
    }

    internal static Event NewEvent(UniqueId id) {
      Event ev;

      ev = (Event)Create(id);

      return ev;
    }

    internal static byte GetTokenId(IProtocolToken obj) {
#if DEBUG
      if (_token2id.ContainsKey(obj.GetType()) == false) {
        throw new BoltException("Unknown token type {0}", obj.GetType());
      }
#endif

      return _token2id[obj.GetType()];
    }

    internal static IProtocolToken NewToken(byte id) {
#if DEBUG
      if (_id2token.ContainsKey(id) == false) {
        throw new BoltException("Unknown token id {0}", id);
      }
#endif

      return (IProtocolToken)Activator.CreateInstance(_id2token[id]);
    }

    internal static Command NewCommand(TypeId id) {
      return (Command)Create(id);
    }

    internal static Command NewCommand(UniqueId id) {
      return (Command)Create(id);
    }

    internal static IEntitySerializer NewSerializer(TypeId id) {
      return (IEntitySerializer)Create(id);
    }

    internal static IEntitySerializer NewSerializer(UniqueId guid) {
      return (IEntitySerializer)Create(guid);
    }

    static object Create(TypeId id) {
#if DEBUG
      if (_factoriesById.ContainsKey(id) == false) {
        BoltLog.Error("Unknown {0}", id);
      }
#endif

      return _factoriesById[id].Create();
    }

    static object Create(UniqueId id) {
#if DEBUG
      if (_factoriesByKey.ContainsKey(id) == false) {
        BoltLog.Error("Unknown {0}", id);
      }
#endif

      return _factoriesByKey[id].Create();
    }

    internal static void UnregisterAll() {
      _token2id.Clear();
      _id2token.Clear();

      _factoriesById.Clear();
      _factoriesByKey.Clear();
      _factoriesByType.Clear();

      _token2id = new Dictionary<Type, byte>();
      _id2token = new Dictionary<byte, Type>();

      _factoriesById = new Dictionary<TypeId, IFactory>(128, TypeId.EqualityComparer.Instance);
      _factoriesByKey = new Dictionary<UniqueId, IFactory>(128, UniqueId.EqualityComparer.Instance);
      _factoriesByType = new Dictionary<Type, IFactory>();
    }

    internal static void RegisterTokenClass(Type type) {
      if (_token2id.Count == 254) {
        throw new ArgumentException("Can only register 254 different token types");
      }

      byte id = (byte)(_token2id.Count + 1);

      _token2id.Add(type, id);
      _id2token.Add(id, type);

      BoltLog.Debug("Registered token class {0} as id {1}", type, id);
    }
  }
}