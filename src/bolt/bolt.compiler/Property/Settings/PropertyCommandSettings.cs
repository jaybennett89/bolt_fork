using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class PropertyCommandSettings : PropertyAssetSettings {
    [ProtoMember(1)]
    public bool SmoothCorrection;
  }
}
