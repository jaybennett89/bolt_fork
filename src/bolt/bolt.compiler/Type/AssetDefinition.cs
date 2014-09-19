using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace bolt.compiler {
  public struct AssetDefinitionCompilationData {
    public uint ClassId;
  }

  [ProtoContract]
  [ProtoInclude(100, typeof(StateDefinition))]
  [ProtoInclude(200, typeof(EventDefinition))]
  [ProtoInclude(300, typeof(ObjectDefinition))]
  [ProtoInclude(400, typeof(CommandDefinition))]
  public abstract class AssetDefinition {
    [ProtoIgnore]
    public bool Deleted;

    [ProtoIgnore]
    public Context Context;

    [ProtoIgnore]
    public AssetDefinitionCompilationData CompilationDataAsset;

    [ProtoMember(1)]
    public Guid AssetGuid;

    [ProtoMember(4)]
    public string AssetPath;

    [ProtoMember(5)]
    public string ClassComment;

    [ProtoMember(6)]
    public bool Enabled;

    public abstract IEnumerable<PropertyDefinition> DefinedProperties {
      get;
    }
  }
}
