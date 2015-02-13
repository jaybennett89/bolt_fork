using UnityEngine;
using System.Collections;
using System;

[BoltGlobalBehaviour]
public class ServerCallbacks : Bolt.GlobalEventListener {
  public override void Connected(BoltConnection connection) {
    BoltNetwork.Instantiate(BoltPrefabs.Client, new Vector3(UnityEngine.Random.value * 16f, 0, UnityEngine.Random.value * 16f), Quaternion.identity);
  }
}
