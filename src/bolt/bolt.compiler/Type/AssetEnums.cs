using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public enum EventTypes {
    Global = 0,
    Entity = 1
  }

  [ProtoContract]
  public enum ReliableModes {
    Reliable = 0,
    Unreliable = 1
  }

  [ProtoContract]
  public enum GlobalReplicationTargets {
    Everyone = 0,
    Others = 1,
    Server = 2,
    Client = 3,
  }

  [ProtoContract]
  public enum GlobalReplicationSenders {
    Everyone = 0,
    ServerOnly = 1,
    ClientOnly = 2
  }

  [ProtoContract]
  public enum EntityReplicationSenders {
    Everyone = 0,
    OwnerOnly = 1,
    ControllerOnly = 2,
    OwnerAndController = 3
  }

  [ProtoContract]
  public enum TransformSpaces {
    World = 0,
    Local = 1
  }

  [ProtoContract]
  public enum TransformRotationModes {
    Quaternion = 0,
    EulerAngles = 1
  }

  [ProtoContract]
  public enum TransformImplementations {
    InterpolatedSnapshots = 0,
    DeadReckoning = 1
  }
}
