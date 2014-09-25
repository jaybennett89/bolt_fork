using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class PropertyFilterDefinition {
    [ProtoMember(1)]
    public int Index;

    [ProtoMember(5)]
    public bool Enabled;

    [ProtoMember(3)]
    public string Name;

    public int Bit {
      get { return 1 << Index; }
    }
  }
}
