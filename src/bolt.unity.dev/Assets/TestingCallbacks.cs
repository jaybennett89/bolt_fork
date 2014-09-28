using UnityEngine;
using System.Collections;

[BoltGlobalBehaviour]
public class TestingCallbacks : BoltCallbacksBase {
  public override void SceneLoadLocalDone(string map) {
    BoltNetwork.Instantiate(BoltPrefabs.Cube, new Vector3(Random.Range(-16f, 16f), 0, 0), Quaternion.identity);
  }
}
