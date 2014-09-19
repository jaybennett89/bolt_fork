using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace bolt.compiler {
  [ProtoContract]
  public class PropertyTypeFloat : PropertyType {
    [ProtoMember(50)]
    public int MinValue;

    [ProtoMember(51)]
    public int MaxValue;

    [ProtoMember(52)]
    public int Bits;

    public override int ByteSize {
      get { return 4; }
    }

    public override bool MecanimUsable {
      get { return true; }
    }

    public override IEnumerable<Type> AssetTypes {
      get {
        yield return typeof(EventDefinition);
        yield return typeof(StateDefinition);
        yield return typeof(CommandDefinition);
      }
    }
  }
}
