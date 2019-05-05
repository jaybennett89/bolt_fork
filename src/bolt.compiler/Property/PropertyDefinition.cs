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
    public bool IsArrayElement;

    [ProtoIgnore]
    public int Adjust;

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
    public bool Controller;

    [ProtoMember(11)]
    public ReplicationMode ReplicationMode;

    [ProtoMember(7)]
    public string Comment;

    [ProtoMember(9)]
    public int Priority;

    [ProtoMember(2)]
    public PropertyType PropertyType;

    [ProtoMember(6)]
    public PropertyAssetSettings AssetSettings;

    [ProtoMember(8)]
    public int Filters;

    public PropertyStateSettings StateAssetSettings {
      get { return AssetSettings as PropertyStateSettings; }
    }

    public PropertyEventSettings EventAssetSettings {
      get { return AssetSettings as PropertyEventSettings; }
    }

    public PropertyCommandSettings CommandAssetSettings {
      get { return AssetSettings as PropertyCommandSettings; }
    }

    public void Oncreated() {
      Enabled = true;
      Expanded = true;

      Priority = 1;

      if (StateAssetSettings != null) {
        StateAssetSettings.SnapMagnitude = 10f;
      }

      PropertyType.OnCreated();
    }
  }
}
