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

  void TeamChanged() {
    switch (state.team) {
      case Player.TEAM_RED: text.renderer.material.color = Color.red; break;
      case Player.TEAM_BLUE: text.renderer.material.color = Color.blue; break;
    }
  }

  void Update() {
    text.renderer.enabled = !entity.hasControl;

    if (!entity.hasControl) {
      try {
        transform.LookAt(PlayerCamera.instance.transform);
        transform.rotation = Quaternion.LookRotation(-transform.forward);
      }
      catch (Exception exn) {
        BoltLog.Exception(exn);
      }
    }
  }

  public override void Attached() {
    state.nameChanged += NameChanged;
    state.teamChanged += TeamChanged;
  }
}
