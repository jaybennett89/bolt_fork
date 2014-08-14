using UnityEngine;

[BoltGlobalBehaviour(BoltNetworkModes.Server, "Sample_RootMotion")]
public class RootMotionSpawnScript : BoltCallbacks {
  public override void MapLoadDone (string arg) {
    BoltNetwork.Instantiate(BoltPrefabs.RootMotionChar);
  }
}
