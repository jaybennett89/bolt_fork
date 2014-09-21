using ProtoBuf;

namespace Bolt.Compiler {
  [ProtoContract]
  public enum ReplicationTargets {
    Everyone = 0,
    EveryoneExceptController = 1,
    OwnerAndController = 2,
    OwnerOnly = 3,
  }

  [ProtoContract]
  public enum ReplicationConditions {
    ValueChanged = 0,
    FirstReplication = 1
  }

  [ProtoContract]
  public enum PropertyMecanimValueType {
    None = 0,
    Parameter = 1,
    LayerWeight = 2,
    Curve = 3
  }

  [ProtoContract]
  public enum PropertyStateAssetOptions {
    ChangedCallback = 0,
    Interpolate = 1,
    Extrapolate = 2,
  }

  [ProtoContract]
  public enum TransformModes {
    InterpolatedSnapshots = 0,
    DeadReckoning = 1
  }
}
