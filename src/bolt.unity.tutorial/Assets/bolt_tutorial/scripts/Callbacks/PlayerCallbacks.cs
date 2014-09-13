using UnityEngine;
using System.Collections;

[BoltGlobalBehaviour]
public class PlayerCallbcks : BoltCallbacks {
  public override void SceneLoadLocalDone(string map) {
    GameUI.Instantiate();
  }

  public override void ControlOfEntityGained(BoltEntity arg) {
    PlayerCamera.instance.SetTarget(arg);
  }
}
