using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace bolt.compiler {
  [ProtoContract]
  public class TypeParameterDefinition {
    [ProtoMember(1)]
    public Guid InstanceGuid;
  }
}
