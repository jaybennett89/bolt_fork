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

    [ProtoMember(2)]
    public Guid Guid;

    [ProtoMember(3)]
    public string Name;

    [ProtoMember(4)]
    public bool IsDefault;
  }
}
