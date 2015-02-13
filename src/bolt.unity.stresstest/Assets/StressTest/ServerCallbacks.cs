using UnityEngine;
using System.Collections;

[BoltGlobalBehaviour(BoltNetworkModes.Server)]
public class ServerCallbacks : Bolt.GlobalEventListener {

  float RandomPos() {
    float v;
    
    v = Random.value;
    v = v - 0.5f;
    v = v * 2f;
    v = v * 16f;

    return v;
  }

  public override void SceneLoadLocalDone(string map) {
    for (int i = 0; i < 256; ++i) {
      BoltNetwork.Instantiate(BoltPrefabs.Sphere, new Vector3(RandomPos(), 0, RandomPos()), Quaternion.identity);
    }
  }
}
