using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class CommandDefinition : AssetDefinition {
    [ProtoMember(50)]
    public List<PropertyDefinition> Input = new List<PropertyDefinition>();

    [ProtoMember(51)]
    public List<PropertyDefinition> Result = new List<PropertyDefinition>();

    [ProtoMember(52)]
    public int SmoothFrames;

    [ProtoMember(60)]
    public bool CompressZeroValues;

    [ProtoIgnore]
    public List<PropertyDefinition> Properties = new List<PropertyDefinition>();

    public override IEnumerable<Type> AllowedPropertyTypes {
      get { return EventDefinition.AllowedEventAndCommandPropertyTypes(); }
    }
  }
}
