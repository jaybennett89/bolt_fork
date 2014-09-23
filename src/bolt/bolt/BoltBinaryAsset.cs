using UnityEngine;

public enum BoltBinaryAssetTypes {
  State = 0,
  Struct = 1,
  Event = 2,
  Command = 3
}

public class BoltBinaryAsset : ScriptableObject {
  [SerializeField]
  public BoltBinaryAssetTypes Type = BoltBinaryAssetTypes.State;

  [SerializeField]
  public byte[] Data = new byte[0];

  [HideInInspector]
  public object UserToken;
}
