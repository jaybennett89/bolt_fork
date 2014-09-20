using UnityEngine;

public class PlayerIK : BoltEntityBehaviour<IPlayerState> {
  // weight blending
  float weight = 0f;
  float weightto = 0f;
  float weightfrom = 0f;
  float weightacc = 0f;

  // if we are aiming or not
  bool _aiming;

  // the animator component
  Animator _animator;

  public override void Attached() {
    _animator = GetComponent<Animator>();
  }

  void OnAnimatorIK() {
    //TODO: FIX PITCH OFFSET CALCULATION

    float pitchOffsetArm = -state.pitch * 0.025f;
    float pitchOffsetHead = -state.pitch * 0.01f;

    _animator.SetLookAtPosition(transform.position + transform.forward + new Vector3(0, 1.5f + pitchOffsetHead, 0f));
    _animator.SetLookAtWeight(1f);

    if (_aiming) {
      if (state.mecanim.Aiming == false) {
        weightacc = 0f;
        weightfrom = weight;
        weightto = 0f;
        _aiming = false;
      }
    }
    else {
      if (state.mecanim.Aiming) {
        weightacc = 0f;
        weightfrom = weight;
        weightto = 1f;
        _aiming = true;
      }
    }

    // need to re-enable this

    //weightacc += Time.deltaTime;
    //weight = Mathf.Lerp(weightfrom, weightto, weightacc / 0.4f);

    //Vector3 pos = transform.position + transform.forward + new Vector3(0, 1.5f + pitchOffsetArm, 0) + (transform.right * 0.3f);
    //Quaternion rot = transform.parent.rotation * Quaternion.Euler(state.pitch, 0, -90);

    //_animator.SetIKPosition(AvatarIKGoal.RightHand, pos);
    //_animator.SetIKRotation(AvatarIKGoal.RightHand, rot);

    //_animator.SetIKPositionWeight(AvatarIKGoal.RightHand, weight);
    //_animator.SetIKRotationWeight(AvatarIKGoal.RightHand, weight);
  }
}