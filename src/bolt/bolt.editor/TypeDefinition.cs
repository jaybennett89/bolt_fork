using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace bolt.compiler {
  [ProtoContract]
  [ProtoInclude(100, typeof(ActorDefinition))]
  public abstract class TypeDefinition {
    [ProtoIgnore]
    public bool Deleted;

    [ProtoMember(1)]
    public Guid TypeGuid;

    [ProtoMember(2)]
    public Guid ParentGuid;

    [ProtoMember(3)]
    public bool IsAbstract;

    [ProtoMember(4)]
    public string AssetPath;
  }
}
