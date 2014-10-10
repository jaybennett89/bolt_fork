using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class EventDefinition : AssetDefinition {
    [ProtoMember(50)]
    public List<PropertyDefinition> Properties = new List<PropertyDefinition>();

    [ProtoMember(52)]
    public EntityEventTargets EntityTargets;

    [ProtoMember(53)]
    public EntityEventSenders EntitySenders;

    [ProtoMember(54)]
    int _globalTargets;

    [ProtoMember(55)]
    public GlobalEventSenders GlobalSenders;

    [ProtoMember(56)]
    public bool Global;

    [ProtoMember(57)]
    public int Filters;

    [ProtoMember(58)]
    public int Priority;

    [ProtoIgnore]
    public GlobalEventTargets GlobalTargets {
      get { return (GlobalEventTargets)_globalTargets; }
      set { _globalTargets = (int)value; }
    }

    public bool Entity {
      get { return !Global; }
    }

    public override IEnumerable<Type> AllowedPropertyTypes {
      get { return AllowedEventAndCommandPropertyTypes(); }
    }

    internal static IEnumerable<Type> AllowedEventAndCommandPropertyTypes() {
      yield return typeof(PropertyTypeEntity);
      yield return typeof(PropertyTypeFloat);
      yield return typeof(PropertyTypeBool);
      yield return typeof(PropertyTypeInteger);
      yield return typeof(PropertyTypeString);
      yield return typeof(PropertyTypeVector);
      yield return typeof(PropertyTypeQuaternion);
      yield return typeof(PropertyTypeColor);
    }
  }
}
