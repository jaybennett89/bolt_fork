using UnityEngine;
using System.Collections;

[BoltGlobalBehaviour(BoltNetworkModes.Server, "Sample_Prioritization")]
public class SphereSpawner : BoltCallbacks {
  public override void MapLoadLocalDone (string arg) {
    for (int i = 0; i < 128; ++i) {
      BoltNetwork.Instantiate(BoltPrefabs.Sphere);
    }
  }
}
