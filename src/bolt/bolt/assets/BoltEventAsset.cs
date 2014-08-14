using UnityEngine;

public class BoltEventAsset : BoltCompilableAsset {
  [HideInInspector]
  public ushort id;
  public int assignedId = -1;
  public BoltAssetEventMode eventMode;
  public BoltEventDeliveryMode deliveryMode;
  public BoltAssetEventGlobalSource globalSource;
  public BoltAssetEventGlobalTarget globalTarget;
  public BoltAssetEventEntitySource entitySource;
  public BoltAssetEventEntityTarget entityTarget;
  public BoltAssetProperty[] properties = new BoltAssetProperty[0];

  public string interfaceName { get { return "I" + name; } }
  public string factoryName { get { return name + "Handler"; } }
  public string receiverName { get { return interfaceName + "Receiver"; } }
}
