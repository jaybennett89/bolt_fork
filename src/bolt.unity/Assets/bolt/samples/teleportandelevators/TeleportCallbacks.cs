using UnityEngine;
using System.Collections;

[BoltGlobalBehaviour("Sample_TeleportAndElevators")]
public class TeleportCallbacks : BoltCallbacks {
  public override void ControlOfEntityGained (BoltEntity arg) {
    TeleportCamera.instance.target = arg.transform;
  }
}
