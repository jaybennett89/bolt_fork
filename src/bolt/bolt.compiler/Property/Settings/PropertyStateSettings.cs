using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class PropertyStateSettings : PropertyAssetSettings {
    [ProtoMember(5)]
    public SmoothingAlgorithms SmoothingAlgorithm;

    [ProtoMember(6)]
    public float ExtrapolationErrorTolerance = 0.25f;

    [ProtoMember(18)]
    public int ExtrapolationCorrectionFrames = 6;

    [ProtoMember(19)]
    public int ExtrapolationMaxFrames = 9;

    [ProtoMember(8)]
    public MecanimMode MecanimMode;

    [ProtoMember(17)]
    public MecanimDirection MecanimDirection;

    [ProtoMember(12)]
    public float MecanimDamping;

    [ProtoMember(14)]
    public int MecanimLayer;
  }
}
