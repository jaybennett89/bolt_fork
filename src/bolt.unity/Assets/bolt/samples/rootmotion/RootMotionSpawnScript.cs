using UnityEngine;
using System.Linq;

[BoltGlobalBehaviour(BoltNetworkModes.Server | BoltNetworkModes.Client, "Sample_RootMotion")]
public class RootMotionSpawnScript : BoltCallbacks {
  public override void SceneLoadLocalDone (string arg) {
    BoltNetwork.Instantiate(BoltPrefabs.RootMotionChar);
  }
}
