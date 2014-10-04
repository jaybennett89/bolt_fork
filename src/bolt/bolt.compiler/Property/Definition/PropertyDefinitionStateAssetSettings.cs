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

    [ProtoMember(7)]
    public bool Mecanim;

    [ProtoMember(8)]
    public float MecanimDamping;
  }
}
