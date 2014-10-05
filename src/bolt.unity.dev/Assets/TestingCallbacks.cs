using UnityEngine;
using System.Collections;

[BoltGlobalBehaviour(BoltNetworkModes.Server)]
public class TestingCallbacks : BoltCallbacksBase {
  public override void SceneLoadLocalDone(string map) {

  }

  public override void SceneLoadRemoteDone(BoltConnection connection, string map) {
    BoltNetwork.Instantiate(BoltPrefabs.Cube, new Vector3(Random.Range(-16f, 16f), 10, Random.Range(-16f, 16f)), Quaternion.identity).AssignControl(connection);
  }
}
