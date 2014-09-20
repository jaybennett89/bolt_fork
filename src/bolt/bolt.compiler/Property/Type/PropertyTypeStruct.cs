using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class PropertyTypeStruct : PropertyType {
    [ProtoMember(50)]
    public Guid ObjectId;

    public StructDefinition Struct {
      get { return Context.FindStruct(ObjectId); }
    }

    public override int ByteSize {
      get { throw new NotImplementedException(); }
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
  }
}
