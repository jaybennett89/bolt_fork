using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bolt.compiler {
  [ProtoContract]
  public class PropertyTypeState : PropertyType {
    [ProtoMember(50)]
    public Guid StateId;

    public StateDefinition State {
      get { return Context.FindState(StateId); }
    }

    public override int ByteSize {
      get { throw new NotImplementedException(); }
    }

    public override bool MecanimUsable {
      get { return false; }
    }

    public override IEnumerable<Type> AssetTypes {
      get { yield return typeof(StateDefinition); }
    }

    public override void CalculateObjectCount(StateDefinition state) {
      state.CompilationDataState.ObjectCount += 1;
    }
  }
}
