using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class PropertyStateSettings : PropertyAssetSettings {
    [ProtoMember(5)]
    public StateEstimationAlgorithm EstimationAlgorithm;

    [ProtoMember(6)]
    public float DeadReckoningErrorTolerance = 0.1f;

    [ProtoMember(8)]
    public MecanimMode MecanimMode;

    [ProtoMember(9)]
    public MecanimDirection MecanimOwnerDirection;

    [ProtoMember(11)]
    public MecanimDirection MecanimControllerDirection;

    [ProtoMember(15)]
    public MecanimDirection MecanimOthersDirection;

    [ProtoMember(12)]
    public float MecanimDamping;

    [ProtoMember(14)]
    public int MecanimLayer;
  }
}
