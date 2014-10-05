using UnityEngine;
using System.Collections;

[BoltGlobalBehaviour(BoltNetworkModes.Server)]
public class TestingCallbacks : BoltCallbacksBase {
  public override void SceneLoadLocalDone(string map) {
    Invoke("TakeAwayControl", 5);
  }

  public override void SceneLoadRemoteDone(BoltConnection connection, string map) {
    BoltEntity entity = BoltNetwork.Instantiate(BoltPrefabs.Cube, new Vector3(-50, 20, 0), Quaternion.identity);
    entity.AssignControl(connection);
  }

  void TakeAwayControl() {
    foreach (BoltEntity entity in BoltNetwork.entities) {
      //entity.RevokeControl();
    }
  }
}
