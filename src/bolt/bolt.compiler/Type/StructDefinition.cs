using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class StructDefinition : AssetDefinition {
    [ProtoMember(50)]
    public List<PropertyDefinition> Properties = new List<PropertyDefinition>();

    public override IEnumerable<Type> AllowedPropertyTypes {
      get {
        yield return typeof(PropertyTypeFloat);
        yield return typeof(PropertyTypeInteger);
        yield return typeof(PropertyTypeStruct);
        yield return typeof(PropertyTypeString);
        yield return typeof(PropertyTypeBool);
        yield return typeof(PropertyTypeEntity);
        yield return typeof(PropertyTypeVector);
        yield return typeof(PropertyTypeQuaternion);
        yield return typeof(PropertyTypeColor);
        yield return typeof(PropertyTypePrefabId);
        yield return typeof(PropertyTypeNetworkId);
      }
    }
  }
}
