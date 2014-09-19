using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bolt.compiler {
  [ProtoContract]
  public class PropertyTypeObject : PropertyType {
    [ProtoMember(50)]
    public Guid ObjectId;

    public ObjectDefinition Object {
      get { return Context.FindObject(ObjectId); }
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
      state.CompilationDataState.ObjectCount += 1;
    }
  }
}
