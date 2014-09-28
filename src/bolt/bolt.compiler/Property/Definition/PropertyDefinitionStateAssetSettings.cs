using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class PropertyDefinitionStateAssetSettings : PropertyDefinitionAssetSettings {
    [ProtoMember(5)]
    public StateInterpolationMode InterpMode;
  }
}
