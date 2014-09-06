using UnityEngine;

public class ClientCubeSerializer : BoltEntitySerializer<IClientCubeState> {
  public override void Attached (IClientCubeState state) {

  }

  void Update () {
    renderer.material.color = boltEntity.boltIsControlling ? Color.green : Color.red;
  }
}
