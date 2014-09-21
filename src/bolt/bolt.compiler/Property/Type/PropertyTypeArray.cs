using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class PropertyTypeArray : PropertyType {
    [ProtoMember(51)]
    public int ElementCount;

    [ProtoMember(52)]
    public PropertyType ElementType;

    public IEnumerable<Type> AllowedElementTypes {
      get {
        yield return typeof(PropertyTypeFloat);
        yield return typeof(PropertyTypeStruct);
      }
    }

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
      return new PropertyDecoratorArray();
    }
  }
}
