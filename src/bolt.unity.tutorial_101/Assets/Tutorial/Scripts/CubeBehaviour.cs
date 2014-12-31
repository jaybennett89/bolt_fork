using UnityEngine;
using System.Collections;

public class CubeBehaviour : Bolt.EntityBehaviour<ICubeState> {
  public override void Attached() {
    state.CubeTransform.SetTransforms(transform);

    if (entity.isOwner) {
      state.CubeColor = new Color(Random.value, Random.value, Random.value);
    }

    state.AddCallback("CubeColor", ColorChanged);
  }

  void ColorChanged() {
    renderer.material.color = state.CubeColor;
  }

  public override void SimulateOwner() {
    var speed = 4f;
    var movement = Vector3.zero;

    if (Input.GetKey(KeyCode.W)) { movement.z += 1; }
    if (Input.GetKey(KeyCode.S)) { movement.z -= 1; }
    if (Input.GetKey(KeyCode.A)) { movement.x -= 1; }
    if (Input.GetKey(KeyCode.D)) { movement.x += 1; }

    if (movement != Vector3.zero) {
      transform.position = transform.position + (movement.normalized * speed * BoltNetwork.frameDeltaTime);
    }
  }

  void OnGUI() {
    if (entity.isOwner) {
      GUI.color = state.CubeColor;
      GUILayout.Label("@@@");
      GUI.color = Color.white;
    }
  }
}
