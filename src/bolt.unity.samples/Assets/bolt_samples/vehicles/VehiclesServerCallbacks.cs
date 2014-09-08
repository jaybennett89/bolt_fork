using UnityEngine;
using System.Collections;

[BoltGlobalBehaviour(BoltNetworkModes.Server, "Sample_Vehicles")]
public class VehiclesServerCallbacks : BoltCallbacks {
  public override void EntityAttached (BoltEntity arg) {
    if (arg.GetComponent<CarControl>()) {
      // cars should not be kinematic on the server
      arg.GetComponent<Rigidbody>().isKinematic = false;
    }
  }

  public override void SceneLoadRemoteDone (BoltConnection arg, string map) {
    CarConnectionToken token = (CarConnectionToken) arg.userToken;
    token.character = BoltNetwork.Instantiate(BoltPrefabs.CarPlayer);
    token.character.transform.position = new Vector3(Random.Range(-8f, 8f), 0, Random.Range(-8f, 8f));
    token.character.GiveControl(arg);
  }
}
