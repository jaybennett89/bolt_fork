using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  [ProtoInclude(100, typeof(PropertyStateSettings))]
  [ProtoInclude(200, typeof(PropertyEventSettings))]
  [ProtoInclude(300, typeof(PropertyCommandSettings))]
  public abstract class PropertyAssetSettings {
  }
}
