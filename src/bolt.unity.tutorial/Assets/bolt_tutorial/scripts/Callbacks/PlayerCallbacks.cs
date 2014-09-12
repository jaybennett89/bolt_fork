using UnityEngine;
using System.Collections;

public class PlayerCallbcks : BoltCallbacks {
  public override void ControlOfEntityGained(BoltEntity arg) {
    BoltLog.Info("You are in control of {0}", arg.name);
  }
}
