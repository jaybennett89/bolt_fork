using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  [ProtoInclude(100, typeof(PropertyDefinitionStateAssetSettings))]
  [ProtoInclude(200, typeof(PropertyDefinitionEventAssetSettings))]
  public abstract class PropertyDefinitionAssetSettings {

  }
}
