using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Bolt.Compiler {
  [ProtoContract]
  public class StateDefinition : AssetDefinition {
    [ProtoMember(50)]
    public List<PropertyDefinition> Properties = new List<PropertyDefinition>();

    [ProtoMember(53)]
    public bool IsAbstract;

    [ProtoMember(52)]
    public Guid ParentGuid;

    public static StateDefinition Default() {
      StateDefinition s;

      s = new StateDefinition();
      s.Guid = Guid.NewGuid();
      s.Enabled = true;

      return s;
    }
  }
}
