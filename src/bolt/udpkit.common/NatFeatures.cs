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

    public override string ToString() {
      return string.Format("[NatFeatures Lan={0} Wan={1} AllowsUnsolicitedTraffic={2}, HairpinTranslation={3}, EndPointPreservation={4}", LanEndPoint, WanEndPoint, AllowsUnsolicitedTraffic, SupportsHairpinTranslation, SupportsEndPointPreservation);
    }
  }
}
