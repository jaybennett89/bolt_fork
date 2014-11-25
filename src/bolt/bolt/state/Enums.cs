using System;

#if BOLT_COMPILER_DLL
using ProtoBuf;
#endif

#if BOLT_COMPILER_DLL
namespace Bolt.Compiler {
#else
namespace Bolt {
#endif

  
#if BOLT_COMPILER_DLL
  [ProtoContract(EnumPassthru = true)]
  public
#else
  internal
#endif 
  enum AxisSelections {
    XYZ = X | Y | Z,
    XY = X | Y,
    XZ = X | Z,
    YZ = Y | Z,
    X = 1 << 1,
    Y = 1 << 2,
    Z = 1 << 3,
    Disabled = 0,
  }

#if BOLT_COMPILER_DLL
  [ProtoContract(EnumPassthru = true)]
  public
#else
  internal
#endif
 enum ReplicationMode {
    EveryoneExceptController = 0,
    Everyone = 1,
    OnlyOwnerAndController = 2,
    OnlyOwner = 3
  }

#if BOLT_COMPILER_DLL
  [ProtoContract(EnumPassthru = true)]
  public
#else
  internal
#endif
 enum ExtrapolationVelocityModes {
    CalculateFromPosition = 0,
    CopyFromRigidbody = 1,
    CopyFromRigidbody2D = 2,
    CopyFromCharacterController = 3
  }

#if BOLT_COMPILER_DLL
  [ProtoContract(EnumPassthru = true)]
  public
#else
  internal
#endif
 enum SmoothingAlgorithms {
    None = 0,
    Interpolation = 1,
    Extrapolation = 2,
  }

#if BOLT_COMPILER_DLL
  [ProtoContract(EnumPassthru = true)]
  public
#else
  internal
#endif
 enum TransformSpaces {
    Local = 0,
    World = 1,
  }

#if BOLT_COMPILER_DLL
  [ProtoContract(EnumPassthru = true)]
  public
#else
  internal
#endif
 enum TransformRotationMode {
    QuaternionComponents = 0,
    EulerAngles = 1,
  }


#if BOLT_COMPILER_DLL
  [ProtoContract(EnumPassthru = true)]
  public
#else
  internal
#endif
 enum MecanimMode {
    Disabled,
    Parameter,
    LayerWeight
  }

#if BOLT_COMPILER_DLL
  [ProtoContract(EnumPassthru = true)]
  public
#else
  internal
#endif
 enum MecanimDirection {
    UsingAnimatorMethods,
    UsingBoltProperties
  }
#if BOLT_COMPILER_DLL
  [ProtoContract(EnumPassthru = true)]
  public
#else
  internal
#endif
 enum StringEncodings {
    ASCII = 0,
    UTF8 = 1
  }
}
