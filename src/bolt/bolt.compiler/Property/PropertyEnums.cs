using ProtoBuf;

namespace Bolt.Compiler {
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
