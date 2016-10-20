#pragma warning disable 1591

public enum BoltAssetPropertyEditMode {
  State,
  Event,
  Mecanim,
  Command
}

public enum BoltAssetAxes {
  X = 1,
  Y = 2,
  Z = 4,
  W = 8
}

public enum BoltAssetTransformVelocityMode {
  //InferFromPosition,
  SendFromOwner
}

public enum BoltAssetTransformAccelerationMode {
  DontUse,
  InferFromVelocity,
  SendFromOwner
}

public enum BoltAssetEventMode {
  Global,
  Entity
}

public enum BoltAssetEventGlobalTarget {
  Client = 1,
  Server = 2,
  Sender = 4
}

public enum BoltAssetEventGlobalSource {
  Client = 1,
  Server = 2,
}

public enum BoltAssetEventEntityTarget {
  Owner = 1,
  Controller = 2,
  Proxy = 4
}

public enum BoltAssetEventEntitySource {
  Owner = 1,
  Controller = 2,
  Proxy = 4
}

public enum BoltAssetPropertyType {
  Bool,
  Byte,
  UShort,
  Int,
  Long,
  Float,
  Color,
  Color32,
  Vector2,
  Vector3,
  Vector4,
  Quaternion,
  Entity,
  String,
  Transform,
  Mecanim,
  Custom,
  Trigger,
  ByteArray,
  UniqueId
}

public enum BoltAssetQuaternionMode {
  Components,
  Angles,
}

public enum BoltAssetFloatCompression {
  None,
  Half,
  ByteAngle,
  ByteAngle180,
  ByteNegOneOne,
  ByteZeroOne,
}

public enum BoltAssetStringEncoding {
  ASCII,
  UTF8
}

public enum BoltAssetSyncMode {
  Changed,
  OnceOnInit,
}

public enum BoltAssetSyncTarget {
  Proxy = 1,
  Controller = 2,
}

public enum BoltAssetPropertyOptions {
  Notify = 1,
}

public enum BoltAssetTransformModes {
  InterpolatedSnapshots,
  DeadReckoning
}

#pragma warning restore 1591