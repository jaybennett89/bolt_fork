using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace bolt.compiler {
  [ProtoContract]
  public class PropertyDefinition {
    [ProtoIgnore]
    public bool Deleted;

    [ProtoMember(1)]
    public Guid InstanceGuid;

    [ProtoMember(2)]
    public PropertyType PropertyType;

    [ProtoMember(3)]
    public bool Enabled;

    [ProtoMember(4)]
    public bool Replicated;

    [ProtoMember(5)]
    public bool Expanded;

    [ProtoMember(6)]
    public PropertyDefinitionAssetSettings AssetSettings;

    [ProtoMember(7)]
    public string Comment;
  }
}
