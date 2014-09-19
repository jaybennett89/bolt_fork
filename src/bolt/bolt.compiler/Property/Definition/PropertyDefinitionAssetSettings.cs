using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace bolt.compiler {
  [ProtoContract]
  [ProtoInclude(100, typeof(PropertyDefinitionStateAssetSettings))]
  [ProtoInclude(200, typeof(PropertyDefinitionEventAssetSettings))]
  public abstract class PropertyDefinitionAssetSettings {

  }
}
