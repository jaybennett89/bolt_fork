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

    [ProtoMember(16)]
    public bool MecanimEnabled;

    public string GetMecanimDataExpression() {
      return string.Format("new Bolt.PropertyMecanimData {{ Mode = Bolt.MecanimMode.{0}, OwnerDirection = Bolt.MecanimDirection.{1}, ControllerDirection = Bolt.MecanimDirection.{2}, OthersDirection = Bolt.MecanimDirection.{3}, Layer = {4}, Damping = {5}f, Enabled = {6} }}", MecanimMode, MecanimOwnerDirection, MecanimControllerDirection, MecanimOthersDirection, MecanimLayer, MecanimDamping, MecanimEnabled.ToString().ToLower());
    }
  }
}
