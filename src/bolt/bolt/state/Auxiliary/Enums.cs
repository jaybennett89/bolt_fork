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
  [ProtoContract]
  public
#else
  internal
#endif
 enum StateEstimationAlgorithm {
    None = 0,
    InterpolatedSnapshots = 1,
    DeadReckoning = 2,
  }

#if BOLT_COMPILER_DLL
  [ProtoContract]
  public
#else
  internal
#endif
 enum TransformSpaces {
    Local = 0,
    World = 1,
  }

#if BOLT_COMPILER_DLL
  [ProtoContract]
  public
#else
  internal
#endif
 enum TransformRotationMode {
    QuaternionComponents = 0,
    EulerAngles = 1,
  }

  
#if BOLT_COMPILER_DLL
  [ProtoContract]
  public
#else
  internal
#endif
 enum MecanimMode {
    None,
    Property,
    LayerWeight
  }

#if BOLT_COMPILER_DLL
  [ProtoContract]
  public
#else
  internal
#endif
 enum MecanimDirection {
    None,
    Push,
    Pull,
  }
}
