using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace bolt.compiler {
  [ProtoContract]
  [ProtoInclude(100, typeof(StateDefinition))]
  [ProtoInclude(200, typeof(EventDefinition))]
  [ProtoInclude(300, typeof(CommandDefinition))]
  public abstract class AssetDefinition {
    [ProtoIgnore]
    public bool Deleted;

    [ProtoMember(1)]
    public Guid InstanceGuid;

    [ProtoMember(2)]
    public Guid ParentGuid;

    [ProtoMember(3)]
    public bool IsAbstract;

    [ProtoMember(4)]
    public string AssetPath;

    [ProtoMember(5)]
    public string Comment;

    [ProtoMember(6)]
    public bool Enabled;

    [ProtoMember(7)]
    public List<PropertyDefinition> Properties = new List<PropertyDefinition>();
  }
}
