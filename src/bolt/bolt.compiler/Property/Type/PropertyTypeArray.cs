using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bolt.compiler {
  [ProtoContract]
  public class PropertyTypeArray : PropertyType {
    [ProtoMember(50)]
    public PropertyType ElementType;

    [ProtoMember(51)]
    public int ElementCount;

    public IEnumerable<Type> AllowedElementTypes {
      get {
        yield return typeof(PropertyTypeFloat);
        yield return typeof(PropertyTypeObject);
      }
    }

    public override bool IsValue {
      get { return false; }
    }

    public override int ByteSize {
      get { throw new NotImplementedException(); }
    }

    public override IEnumerable<Type> AssetTypes {
      get {
        yield return typeof(StateDefinition);
        yield return typeof(ObjectDefinition);
      }
    }

    public override void CalculateObjectCount(StateDefinition state) {
      if (ElementType is PropertyTypeObject) {
        state.CompilationDataState.ObjectCount += ElementCount;
      }
    }
  }
}
