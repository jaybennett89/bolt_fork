using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Bolt.Compiler {
  [ProtoContract]
  [ProtoInclude(100, typeof(StateDefinition))]
  [ProtoInclude(200, typeof(EventDefinition))]
  [ProtoInclude(300, typeof(StructDefinition))]
  [ProtoInclude(400, typeof(CommandDefinition))]
  public abstract class AssetDefinition : INamedAsset {
    [ProtoIgnore]
    public bool Deleted;

    [ProtoIgnore]
    public Project Project;

    [ProtoMember(2)]
    public string Name;

    [ProtoMember(1)]
    public Guid Guid;

    [ProtoMember(5)]
    public string Comment;

    [ProtoMember(6)]
    public bool Enabled;

    public abstract IEnumerable<Type> AllowedPropertyTypes {
      get;
    }

    string INamedAsset.GetName() {
      return Name;
    }
  }
}
