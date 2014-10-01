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
  public enum MecanimPropertyTypes {
    Parameter = 0,
    LayerWeight = 1,
  }

  [ProtoContract]
  public enum MecanimDampMode {
    None = 0,
    Static = 1,
    Dynamic = 2
  }

  [ProtoContract]
  public enum StateEstimationAlgorithm {
    None = 0,
    InterpolatedSnapshots = 1,
    DeadReckoning = 2,
  }

  [ProtoContract]
  public enum TransformSpaces {
    Local = 0,
    World = 1,
  }

  [ProtoContract]
  public enum TransformRotationMode {
    QuaternionComponents = 0,
    EulerAngles = 1,
  }
}
