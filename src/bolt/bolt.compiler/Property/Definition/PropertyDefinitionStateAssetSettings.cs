using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bolt.Compiler {

  [ProtoContract]
  public class PropertyDefinitionStateAssetSettings : PropertyDefinitionAssetSettings {
    [ProtoContract]
    public class MecanimSettings {
      [ProtoMember(1)]
      public float StaticDampTime;

      [ProtoMember(2)]
      public string ParameterName;

      [ProtoMember(3)]
      public int LayerIndex;

      [ProtoMember(4)]
      public MecanimDampMode MecanimDampMode;

      [ProtoMember(5)]
      public MecanimPropertyTypes PropertyType;
    }

    [ProtoMember(1)]
    public ReplicationConditions Condition;

    [ProtoMember(2)]
    public HashSet<Guid> Filters = new HashSet<Guid>();

    [ProtoMember(3)]
    public HashSet<StatePropertyOptions> Options = new HashSet<StatePropertyOptions>();
  }
}
