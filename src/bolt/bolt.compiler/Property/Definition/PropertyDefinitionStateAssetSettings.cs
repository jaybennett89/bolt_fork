using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace bolt.compiler {
  [ProtoContract]
  public class PropertyDefinitionStateAssetSettings : PropertyDefinitionAssetSettings {
    [ProtoMember(1)]
    public float MecanimDampTime;

    [ProtoMember(2)]
    public string MecanimParameterName;

    [ProtoMember(3)]
    public int MecanimLayerIndex;

    [ProtoMember(4)]
    public PropertyMecanimValueType MecanimValueType;

    [ProtoMember(5)]
    public ReplicationTargets ReplicationTargets;

    [ProtoMember(6)]
    public ReplicationConditions ReplicationCondition;

    [ProtoMember(7)]
    public HashSet<PropertyStateAssetOptions> Options = new HashSet<PropertyStateAssetOptions>();
  }
}
