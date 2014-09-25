using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class PropertyDefinitionStateAssetSettings : PropertyDefinitionAssetSettings {
    [ProtoMember(2)]
    public int Filters;

    [ProtoMember(4)]
    public bool Callback;

    [ProtoMember(5)]
    public StateInterpolationMode InterpMode;
  }
}
