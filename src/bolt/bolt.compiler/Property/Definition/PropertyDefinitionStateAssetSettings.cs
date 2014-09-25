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
      public int LayerIndex;

      [ProtoMember(2)]
      public string ParameterName;

      [ProtoMember(5)]
      public MecanimPropertyTypes PropertyType;
    }

    [ProtoMember(2)]
    public int Filters;

    [ProtoMember(3)]
    public HashSet<StatePropertyOptions> Options = new HashSet<StatePropertyOptions>();
  }
}
