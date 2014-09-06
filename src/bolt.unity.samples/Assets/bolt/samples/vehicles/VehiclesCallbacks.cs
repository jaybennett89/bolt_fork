using UnityEngine;
using System.Collections;

[BoltGlobalBehaviour("Sample_Vehicles")]
public class VehiclesCallbacks : BoltCallbacks {
  //public override void StartDone () {
  //  BoltNetwork.resolveTransform = id => {
  //    foreach (var c in FindObjectsOfType<CarControl>()) {
  //      if (c.boltState.carId == id) { return c.transform; }
  //    }
  //  
  //    return null;
  //  };
  //  
  //  BoltNetwork.resolveTransformId = t => {
  //    return t.GetComponent<CarControl>().boltState.carId;
  //  };
  //}

  public override void Connected (BoltConnection arg) {
    arg.userToken = new CarConnectionToken();
  }

  public override void ControlOfEntityGained (BoltEntity arg) {
    TeleportCamera.instance.target = arg.transform;
  }
}
