namespace UdpKit {
  public enum NatFeatureStates {
    Unknown,
    Yes,
    No
  }

  public class NatFeatures {
    public UdpEndPoint WanEndPoint;
    public UdpEndPoint LanEndPoint;
    public NatFeatureStates AllowsUnsolicitedTraffic;
    public NatFeatureStates SupportsHairpinTranslation;
    public NatFeatureStates SupportsEndPointPreservation;

    public NatFeatures Clone() {
      return (NatFeatures)MemberwiseClone();
    }
  }
}
