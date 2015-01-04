using UnityEngine;
using System.Collections;

public class CubeBehaviour : Bolt.EntityEventListener<ICubeState> {
  public GameObject[] WeaponObjects;

  public override void Attached() {
    state.CubeTransform.SetTransforms(transform);

    if (entity.isOwner) {
      state.CubeColor = new Color(Random.value, Random.value, Random.value);

      // on the owner we also want to setup the weapons, we randomize one weapon from the available ones and also ammo between 50 to 100
      for (int i = 0; i < state.WeaponsArray.Length; ++i) {
        state.WeaponsArray[i].WeaponId = Random.Range(0, WeaponObjects.Length);
        state.WeaponsArray[i].WeaponAmmo = Random.Range(50, 100);
      }

      // by default we don't have any weapon up, so set index to -1
      state.WeaponActiveIndex = -1;
    }

    state.AddCallback("CubeColor", ColorChanged);

    // we also setup a callback for whenever the index changes
    state.AddCallback("WeaponActiveIndex", WeaponActiveIndexChanged);
  }

  void ColorChanged() {
    renderer.material.color = state.CubeColor;
  }

  void WeaponActiveIndexChanged() {
    for (int i = 0; i < WeaponObjects.Length; ++i) {
      WeaponObjects[i].SetActive(false);
    }

    if (state.WeaponActiveIndex >= 0) {
      int objectId = state.WeaponsArray[state.WeaponActiveIndex].WeaponId;
      WeaponObjects[objectId].SetActive(true);
    }
  }

  public override void SimulateOwner() {
    var speed = 4f;
    var movement = Vector3.zero;

    if (Input.GetKey(KeyCode.W)) { movement.z += 1; }
    if (Input.GetKey(KeyCode.S)) { movement.z -= 1; }
    if (Input.GetKey(KeyCode.A)) { movement.x -= 1; }
    if (Input.GetKey(KeyCode.D)) { movement.x += 1; }

    // NEW: Input polling for weapon selection
    if (Input.GetKeyDown(KeyCode.Alpha1)) state.WeaponActiveIndex = 0;
    if (Input.GetKeyDown(KeyCode.Alpha2)) state.WeaponActiveIndex = 1;
    if (Input.GetKeyDown(KeyCode.Alpha3)) state.WeaponActiveIndex = 2;
    if (Input.GetKeyDown(KeyCode.Alpha0)) state.WeaponActiveIndex = -1;

    if (movement != Vector3.zero) {
      transform.position = transform.position + (movement.normalized * speed * BoltNetwork.frameDeltaTime);
    }

    if (Input.GetKeyDown(KeyCode.F)) {
      var flash = FlashColorEvent.Create(entity);
      flash.FlashColor = Color.red;
      flash.Send();
    }
  }

  float resetColorTime;

  public override void OnEvent(FlashColorEvent evnt) {
    resetColorTime = Time.time + 4f;
    renderer.material.color = evnt.FlashColor;
  }

  void Update() {
    if (resetColorTime < Time.time) {
      renderer.material.color = state.CubeColor;
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
