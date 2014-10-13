using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Bolt.Compiler {
  [ProtoContract]
  public class StateDefinition : AssetDefinition {
    [ProtoMember(50)]
    public List<PropertyDefinition> Properties = new List<PropertyDefinition>();

    [ProtoMember(53)]
    public bool IsAbstract;

    [ProtoMember(52)]
    public Guid ParentGuid;

    [ProtoMember(54)]
    public int PacketMaxBits = 512;

    [ProtoMember(55)]
    public int PacketMaxProperties = 16;

    public override IEnumerable<Type> AllowedPropertyTypes {
      get { return AllowedStateAndStructPropertyTypes(); }
    }

    public static StateDefinition Default() {
      StateDefinition s;

      s = new StateDefinition();
      s.Guid = Guid.NewGuid();
      s.Enabled = true;

      return s;
    }

    internal static IEnumerable<Type> AllowedStateAndStructPropertyTypes() {
      yield return typeof(PropertyTypeFloat);
      yield return typeof(PropertyTypeInteger);
      yield return typeof(PropertyTypeArray);
      yield return typeof(PropertyTypeStruct);
      yield return typeof(PropertyTypeString);
      yield return typeof(PropertyTypeTrigger);
      yield return typeof(PropertyTypeTransform);
      yield return typeof(PropertyTypeBool);
      yield return typeof(PropertyTypeEntity);
      yield return typeof(PropertyTypeVector);
      yield return typeof(PropertyTypeQuaternion);
      yield return typeof(PropertyTypeColor);
      yield return typeof(PropertyTypePrefabId);
    }
  }
}

