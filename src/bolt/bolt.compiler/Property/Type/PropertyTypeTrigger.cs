using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bolt.compiler {
  [ProtoContract]
  public class PropertyTypeTrigger : PropertyType {
    public override int ByteSize {
      get { return 4; }
    }
  }
}
