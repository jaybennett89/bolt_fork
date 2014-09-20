using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class EventDefinition : AssetDefinition {
    [ProtoMember(50)]
    public List<PropertyDefinition> Properties = new List<PropertyDefinition>();

    [ProtoMember(51)]
    public EventTypes EventType;

    [ProtoMember(56)]
    public ReliableModes DeliveryMode;

    [ProtoMember(52)]
    public ReplicationTargets EntityTargets;

    [ProtoMember(53)]
    public EntityReplicationSenders EntitySenders;

    [ProtoMember(54)]
    public GlobalReplicationTargets GlobalTargets;

    [ProtoMember(55)]
    public GlobalReplicationSenders GlobalSenders;
  }
}
