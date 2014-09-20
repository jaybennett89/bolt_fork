using UnityEngine;
using System.Collections;

public class PlayerController : BoltEntityBehaviour<IPlayerState> {
  const float MOUSE_SENSEITIVITY = 2f;

  PlayerCommand.Input _input;
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
  }

  void PollKeys(bool mouse) {
    _input.forward = Input.GetKey(KeyCode.W);
    _input.backward = Input.GetKey(KeyCode.S);
    _input.left = Input.GetKey(KeyCode.A);
    _input.right = Input.GetKey(KeyCode.D);
    _input.jump = Input.GetKey(KeyCode.Space);
    _input.aiming = Input.GetMouseButton(1);
    _input.fire = Input.GetMouseButton(0);

    if (Input.GetKeyDown(KeyCode.Alpha1)) {
      _input.weapon = 0;
    }
    else if (Input.GetKeyDown(KeyCode.Alpha2)) {
      _input.weapon = 1;
    }

    if (mouse) {
      _input.yaw += (Input.GetAxisRaw("Mouse X") * MOUSE_SENSEITIVITY);
      _input.yaw %= 360f;

      _input.pitch += (-Input.GetAxisRaw("Mouse Y") * MOUSE_SENSEITIVITY);
      _input.pitch = Mathf.Clamp(_input.pitch, -85f, +85f);
    }
  }

  void DeadChanged() {
    state.mecanim.Dead = state.dead;
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
    activeWeapon.fireFrame = BoltNetwork.serverFrame;
  }

  public override void Attached() {
    // callbacks
    state.deadChanged += DeadChanged;
    state.weaponChanged += WeaponChanged;
    state.mecanim.onFire += OnFire;

    // setup weapon
    WeaponChanged();
  }

  public override void SimulateOwner() {
    if ((BoltNetwork.frame % 5) == 0 && (state.dead == false)) {
      state.health = (byte)Mathf.Clamp(state.health + 1, 0, 100);
    }
  }

  public override void SimulateController() {
    PollKeys(false);

    PlayerCommand cmd;

    cmd = BoltFactory.NewCommand<PlayerCommand>();
    cmd.input = this._input;

    entity.QueueCommand(cmd);
  }

  public override void ExecuteCommand(BoltCommand c, bool resetState) {
    if (state.mecanim.Dead) {
      return;
    }

    PlayerCommand cmd = (PlayerCommand)c;

    if (resetState) {
      _motor.SetState(cmd.state);
    }
    else {
      // move and save the resulting state
      cmd.state = _motor.Move(cmd.input);

      if (cmd.isFirstExecution) {
        // animation
        AnimatePlayer(cmd);

        // set state pitch
        state.pitch = cmd.input.pitch;
        state.weapon = cmd.input.weapon;
        state.mecanim.Aiming = cmd.input.aiming;

        // deal with weapons
        if (cmd.input.aiming && cmd.input.fire) {
          FireWeapon(cmd);
        }
      }
    }
  }

  void AnimatePlayer(PlayerCommand cmd) {
    // FWD <> BWD movement
    if (cmd.input.forward ^ cmd.input.backward) {
      state.mecanim.MoveZ = cmd.input.forward ? 1 : -1;
    }
    else {
      state.mecanim.MoveZ = 0;
    }

    // LEFT <> RIGHT movement
    if (cmd.input.left ^ cmd.input.right) {
      state.mecanim.MoveX = cmd.input.right ? 1 : -1;
    }
    else {
      state.mecanim.MoveX = 0;
    }

    // JUMP
    if (_motor.jumpStartedThisFrame) {
      state.mecanim.Jump();
    }
  }

  void FireWeapon(PlayerCommand cmd) {
    if (activeWeapon.fireFrame + activeWeapon.refireRate <= BoltNetwork.serverFrame) {
      // this gets replicated to all proxies
      state.mecanim.Fire();

      // if we are the owner and the active weapon is a hitscan weapon, do logic
      if (entity.isOwner) {
        activeWeapon.OnOwner(cmd, entity);
      }
    }
  }
}
