﻿using UnityEngine;
using System.Collections;

public class WeaponRifle : WeaponBase {
  public override void OnOwner(PlayerCommand cmd, BoltEntity entity) {
    if (entity.isOwner) {
      IPlayerState state = entity.GetBoltState<IPlayerState>();
      PlayerController controller = entity.GetComponent<PlayerController>();

      Vector3 pos;
      Quaternion look;

      // this calculate the looking angle for this specific entity
      PlayerCamera.instance.CalculateCameraAimTransform(entity.transform, state.pitch, out pos, out look);

      // display debug
      Debug.DrawRay(pos, look * Vector3.forward);

      using (var hits = BoltPhysics.Raycast(new Ray(pos, look * Vector3.forward), cmd.serverFrame)) {
        for (int i = 0; i < hits.count; ++i) {
          var hit = hits.GetHit(i);
          var serializer = hit.body.GetBoltSerializer() as PlayerSerializer;

          if ((serializer != null) && (serializer.state.team != state.team)) {
            serializer.ApplyDamage(controller.activeWeapon.damagePerBullet);
          }
        }
      }
    }
  }

  public override void Fx(BoltEntity entity) {
    Vector3 pos;
    Quaternion rot;
    PlayerCamera.instance.CalculateCameraAimTransform(entity.transform, entity.GetBoltState<IPlayerState>().pitch, out pos, out rot);

    Ray r = new Ray(pos, rot * Vector3.forward);
    RaycastHit rh;

    if (Physics.Raycast(r, out rh) && impactPrefab) {
      var en = rh.transform.GetComponent<BoltEntity>();
      var hit = GameObject.Instantiate(impactPrefab, rh.point, Quaternion.LookRotation(rh.normal)) as GameObject;

      if (en) {
        hit.GetComponent<RandomSound>().enabled = false;
      }

      if (trailPrefab) {
        var trailGo = GameObject.Instantiate(trailPrefab, muzzleFlash.position, Quaternion.identity) as GameObject;

        trailGo.transform.rotation = Quaternion.LookRotation((rh.point - muzzleFlash.position).normalized);

        var trail = trailGo.GetComponent<TrailRenderer>();

        trail.rigidbody.AddRelativeForce(Vector3.forward * 500f, ForceMode.VelocityChange);
      }

    }

    GameObject go = (GameObject)GameObject.Instantiate(shellPrefab, shellEjector.position, shellEjector.rotation);
    go.rigidbody.AddRelativeForce(0, 0, 2, ForceMode.VelocityChange);
    go.rigidbody.AddTorque(new Vector3(Random.Range(-32f, +32f), Random.Range(-32f, +32f), Random.Range(-32f, +32f)), ForceMode.VelocityChange);

    // show flash
    muzzleFlash.gameObject.SetActive(true);
  }
}
