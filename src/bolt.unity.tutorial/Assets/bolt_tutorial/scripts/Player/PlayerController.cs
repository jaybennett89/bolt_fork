﻿using UnityEngine;
using System.Collections;
using System.Linq;

public class PlayerController : Bolt.EntityEventListener<IPlayerState> {
  const float MOUSE_SENSEITIVITY = 2f;

  bool forward;
  bool backward;
  bool left;
  bool right;
  bool jump;
  bool aiming;
  bool fire;

  int weapon;

  float yaw;
  float pitch;

  PlayerMotor _motor;

  [SerializeField]
  WeaponBase[] _weapons;

  [SerializeField]
  AudioSource _weaponSfxSource;

  public WeaponBase activeWeapon {
    get { return _weapons[state.weapon]; }
  }

  void Awake() {
    _motor = GetComponent<PlayerMotor>();
  }

  void Update() {
    PollKeys(true);

    if (entity.isOwner && entity.hasControl && Input.GetKey(KeyCode.L)) {
      for (int i = 0; i < 100; ++i) {
        BoltNetwork.Instantiate(BoltPrefabs.SceneCube, new Vector3(Random.value * 512, Random.value * 512, Random.value * 512), Quaternion.identity);
      }
    }
  }

  void PollKeys(bool mouse) {
    forward = Input.GetKey(KeyCode.W);
    backward = Input.GetKey(KeyCode.S);
    left = Input.GetKey(KeyCode.A);
    right = Input.GetKey(KeyCode.D);
    jump = Input.GetKey(KeyCode.Space);
    aiming = Input.GetMouseButton(1);
    fire = Input.GetMouseButton(0);

    if (Input.GetKeyDown(KeyCode.Alpha1)) {
      weapon = 0;
    }
    else if (Input.GetKeyDown(KeyCode.Alpha2)) {
      weapon = 1;
    }

    if (mouse) {
      yaw += (Input.GetAxisRaw("Mouse X") * MOUSE_SENSEITIVITY);
      yaw %= 360f;

      pitch += (-Input.GetAxisRaw("Mouse Y") * MOUSE_SENSEITIVITY);
      pitch = Mathf.Clamp(pitch, -85f, +85f);
    }
  }

  public override void Attached(Bolt.IProtocolToken token) {
    BoltLog.Info("Attached-Token: {0}", token);

    state.transform.SetTransforms(transform);
    state.SetAnimator(GetComponentInChildren<Animator>());

    // setting layerweights 
    state.Animator.SetLayerWeight(0, 1);
    state.Animator.SetLayerWeight(1, 1);

    state.OnFire += OnFire;
    state.AddCallback("weapon", WeaponChanged);

    // setup weapon
    WeaponChanged();
  }

  void WeaponChanged() {
    // setup weapon
    for (int i = 0; i < _weapons.Length; ++i) {
      _weapons[i].gameObject.SetActive(false);
    }

    _weapons[state.weapon].gameObject.SetActive(true);
  }

  void OnFire() {
    // play sfx
    _weaponSfxSource.PlayOneShot(activeWeapon.fireSound);

    GameUI.instance.crosshair.Spread += 0.1f;

    // 
    activeWeapon.Fx(entity);
  }

  public void ApplyDamage(byte damage) {
    if (!state.Dead) {

      state.health -= damage;

      if (state.health > 100 || state.health < 0) {
        state.health = 0;
      }
    }

    if (state.health == 0) {
      entity.controller.GetPlayer().Kill();
    }
  }

  public override void SimulateOwner() {
    if ((BoltNetwork.frame % 5) == 0 && (state.Dead == false)) {
      using (var mod = state.Modify()) {
        mod.health = (byte)Mathf.Clamp(mod.health + 1, 0, 100);
      }
    }
  }

  public override void SimulateController() {
    PollKeys(false);

    IPlayerCommandInput input = PlayerCommand.Create();

    input.forward = forward;
    input.backward = backward;
    input.left = left;
    input.right = right;
    input.jump = jump;

    input.aiming = aiming;
    input.fire = fire;

    input.yaw = yaw;
    input.pitch = pitch;

    input.weapon = weapon;
    input.Token = new TestToken();

    entity.QueueInput(input);
  }

  public override void ExecuteCommand(Bolt.Command c, bool resetState) {
    if (state.Dead) {
      return;
    }

    PlayerCommand cmd = (PlayerCommand)c;

    if (resetState) {
      _motor.SetState(cmd.Result.position, cmd.Result.velocity, cmd.Result.isGrounded, cmd.Result.jumpFrames);
    }
    else {
      // move and save the resulting state
      var result = _motor.Move(cmd.Input.forward, cmd.Input.backward, cmd.Input.left, cmd.Input.right, cmd.Input.jump, cmd.Input.yaw);

      cmd.Result.position = result.position;
      cmd.Result.velocity = result.velocity;
      cmd.Result.jumpFrames = result.jumpFrames;
      cmd.Result.isGrounded = result.isGrounded;

      if (cmd.IsFirstExecution) {
        // animation
        AnimatePlayer(cmd);

        // set state pitch
        using (var mod = state.Modify()) {
          mod.pitch = cmd.Input.pitch;
          mod.weapon = cmd.Input.weapon;
          mod.Aiming = cmd.Input.aiming;

          // deal with weapons
          if (cmd.Input.aiming && cmd.Input.fire) {
            FireWeapon(cmd);
          }
        }
      }

      if (entity.isOwner) {
        cmd.Result.Token = new TestToken();
      }
    }
  }

  void AnimatePlayer(PlayerCommand cmd) {
    using (var mod = state.Modify()) {
      // FWD <> BWD movement
      if (cmd.Input.forward ^ cmd.Input.backward) {
        mod.MoveZ = cmd.Input.forward ? 1 : -1;
      }
      else {
        mod.MoveZ = 0;
      }

      // LEFT <> RIGHT movement
      if (cmd.Input.left ^ cmd.Input.right) {
        mod.MoveX = cmd.Input.right ? 1 : -1;
      }
      else {
        mod.MoveX = 0;
      }

      // JUMP
      if (_motor.jumpStartedThisFrame) {
        mod.Jump();
      }
    }
  }

  void FireWeapon(PlayerCommand cmd) {

    if (activeWeapon.fireFrame + activeWeapon.refireRate <= BoltNetwork.serverFrame) {
      activeWeapon.fireFrame = BoltNetwork.serverFrame;

      state.Fire();

      // if we are the owner and the active weapon is a hitscan weapon, do logic
      if (entity.isOwner) {
        activeWeapon.OnOwner(cmd, entity);
      }
    }
  }
}
