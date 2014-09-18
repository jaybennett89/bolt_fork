using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace bolt.compiler {
  [ProtoContract]
  public class PropertyDefinitionStateAssetSettings : PropertyDefinitionAssetSettings {
    public bool IsMecanimParameter;
    public string MecanimParameterName;
    public float MecanimDampTime;
  }
}
