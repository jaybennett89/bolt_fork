using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class PropertyDefinition {
    [ProtoIgnore]
    public int Nudge;

    [ProtoIgnore]
    public bool Deleted;

    [ProtoIgnore]
    public Project Context;

    [ProtoMember(1)]
    public string Name;

    [ProtoMember(3)]
    public bool Enabled;

    [ProtoMember(4)]
    public bool Replicated;

    [ProtoMember(5)]
    public bool Expanded;

    [ProtoMember(10)]
    public bool ExcludeController;

    [ProtoMember(7)]
    public string Comment;

    [ProtoMember(9)]
    public float Priority;

    [ProtoMember(2)]
    public PropertyType PropertyType;

    [ProtoMember(6)]
    public PropertyDefinitionAssetSettings AssetSettings;

    [ProtoMember(8)]
    public int Filters;

    public string ChangedCallbackName {
      get { return Name + "Changed"; }
    }

    public PropertyDefinitionStateAssetSettings StateAssetSettings {
      get { return (PropertyDefinitionStateAssetSettings)AssetSettings; }
    }

    public PropertyDefinitionEventAssetSettings EventAssetSettings {
      get { return (PropertyDefinitionEventAssetSettings)AssetSettings; }
    }
  }
}
