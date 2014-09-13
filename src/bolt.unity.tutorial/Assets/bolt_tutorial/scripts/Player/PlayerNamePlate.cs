using System;
using UnityEngine;

public class PlayerNamePlate : BoltEntityBehaviour<IPlayerState> {
  [SerializeField]
  Vector3 offset;

  [SerializeField]
  TextMesh text;

  void NameChanged() {
    text.text = state.name;
  }

  void Update() {
    try {
      transform.LookAt(PlayerCamera.instance.transform);
      transform.rotation = Quaternion.LookRotation(-transform.forward);

      //transform.position = ThirdPersonHudCamera.instance.camera.ScreenToWorldPoint(sp);
      //transform.rotation = Quaternion.identity;
    }
    catch (Exception exn) {
      BoltLog.Exception(exn);
    }
  }

  public override void Attached() {
    state.nameChanged += NameChanged;
  }
}
