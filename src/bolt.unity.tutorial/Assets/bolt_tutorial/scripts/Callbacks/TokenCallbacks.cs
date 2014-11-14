using UnityEngine;
using System.Collections;

[BoltGlobalBehaviour]
public class TokenCallbacks : Bolt.GlobalEventListener {
  public override void BoltStarted() {
    BoltNetwork.RegisterTokenClass<TestToken>();
  }
}
