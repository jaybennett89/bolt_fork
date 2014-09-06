using UnityEngine;

[BoltExecutionOrder(5000)]
public class PlayerNameplate : BoltEntityBehaviour<PlayerSerializer, IPlayerState> {
  public override void Attached () {
    boltState.nameChanged += NameChanged;
  }

  void NameChanged () {
    GetComponent<TextMesh>().text = boltState.name;
  }

  void LateUpdate () {
    transform.LookAt(TutorialPlayerCamera.instance.transform);
    transform.forward = -transform.forward;
  }
}
