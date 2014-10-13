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

    [ProtoMember(17)]
    public MecanimDirection MecanimDirection;

    [ProtoMember(12)]
    public float MecanimDamping;

    [ProtoMember(14)]
    public int MecanimLayer;

    #region NOT_USED

    [ProtoMember(9)]
    public MecanimDirection _NOTUSED_2;

    [ProtoMember(11)]
    public MecanimDirection _NOTUSED_1;

    [ProtoMember(15)]
    public MecanimDirection _NOTUSED_0;

    [ProtoMember(16)]
    public bool _NOTUSED_3;

    #endregion

    public string GetMecanimDataExpression() {
      return string.Format("new Bolt.PropertyMecanimData(Bolt.MecanimMode.{0}, Bolt.MecanimDirection.{1}, {2}f, {3})", MecanimMode, MecanimDirection, MecanimDamping, MecanimLayer);
    }
  }
}
