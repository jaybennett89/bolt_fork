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
        yield return typeof(PropertyTypeEntity);
        yield return typeof(PropertyTypeInteger);
        yield return typeof(PropertyTypeTransform);
      }
    }

    public override bool Compilable {
      get {
        if (ElementType == null) {
          return false;
        }

        if (ElementType is PropertyTypeStruct) {
          return ((PropertyTypeStruct)ElementType).StructGuid != Guid.Empty;
        }

        return true;
      }
    }

    public override bool HasPriority {
      get { return ElementType != null && ElementType.GetType() != typeof(PropertyTypeStruct); }
    }

    public override bool IsValue {
      get { return false; }
    }

    public override bool CallbackAllowed {
      get { return false; }
    }

    public override bool InterpolateAllowed {
      get { return false; }
    }

    public override PropertyDecorator CreateDecorator() {
      return new PropertyDecoratorArray();
    }
  }
}
