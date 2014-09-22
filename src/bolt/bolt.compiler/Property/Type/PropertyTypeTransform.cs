using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class PropertyTypeTransform : PropertyType {
    [ProtoMember(1)]
    public TransformSpaces Space;

    public override IEnumerable<Type> AssetTypes {
      get {
        yield return typeof(StateDefinition);
        yield return typeof(StructDefinition);
      }
    }
  }
}
