using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class PropertyTypeStruct : PropertyType {
    [ProtoMember(50)]
    public Guid StructGuid;

    public override bool IsValue {
      get { return false; }
    }

    public override IEnumerable<Type> AssetTypes {
      get {
        yield return typeof(StateDefinition);
        yield return typeof(StructDefinition); 
      }
    }

    public override PropertyDecorator CreateDecorator() {
      return new PropertyDecoratorStruct();
    }
  }
}
