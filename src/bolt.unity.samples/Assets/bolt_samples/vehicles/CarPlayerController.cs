using UnityEngine;

public class CarPlayerController : BoltEntityBehaviour<CarPlayerSerializer, ICarPlayerState> {
  bool left;
  bool right;
  bool forward;
  bool backward;

  float yRotation = 0f;
  float mouseSensitivity = 2f;

  TeleportPlayerMotor motor;
  Renderer[] renderers;
  CharacterController cc;

  void Awake () {
    cc = GetComponentInChildren<CharacterController>();
    motor = GetComponent<TeleportPlayerMotor>();
    renderers = GetComponentsInChildren<Renderer>();
  }

  public override void Attached () {
    boltState.isActiveInWorldChanged = ActiveInWorldChanged;
  }

  void ActiveInWorldChanged () {
    cc.enabled = boltState.isActiveInWorld;
    motor.enabled = boltState.isActiveInWorld;

    for (int i = 0; i < renderers.Length; ++i) {
      renderers[i].enabled = boltState.isActiveInWorld;
    }
  }

  void PollKeys () {
    left = Input.GetKey(KeyCode.A);
    right = Input.GetKey(KeyCode.D);
    forward = Input.GetKey(KeyCode.W);
    backward = Input.GetKey(KeyCode.S);
  }

  void ClearKeys () {
    left = false;
    right = false;
    forward = false;
    backward = false;
  }

  void Update () {
    if (boltEntity.boltIsControlling) {
      PollKeys();

      if (Input.GetButton("Fire2")) {
        yRotation += (Input.GetAxisRaw("Mouse X") * mouseSensitivity);
      }

      if (Input.GetKeyDown(KeyCode.E)) {
        CarController[] cars = FindObjectsOfType<CarController>();

        // look for a car which is close enough to us
        for (int i = 0; i < cars.Length; ++i) {
          if ((cars[i].transform.position - transform.position).magnitude < 5) {
            ICarPlayerEnterCar evnt = BoltFactory.NewEvent<ICarPlayerEnterCar>();
            evnt.car = cars[i].boltEntity;

            boltEntity.Raise(evnt);
            break;
          }
        }
      }

      TeleportCamera.instance.rotateCameraBehindTarget = left || right || forward || backward;
    }
  }

  public override void OnEvent (ICarPlayerEnterCar evnt, BoltConnection cn) {
    if (evnt.car) {
      if (evnt.car.boltIsRemoteControlled) {
        BoltLog.Error("car is already being controlled by {0}", evnt.car.boltRemoteController);
      } else {
        CarConnectionToken token = (CarConnectionToken) cn.userToken;

        if (token.car) {
          BoltLog.Error("{0} is already driving a car", cn);

        } else {
          // assign the token for this connection the car it'll be driving
          token.car = evnt.car;
          token.character.GetComponent<CarPlayerSerializer>().boltState.isActiveInWorld = false;
          token.character.RevokeControl();

          // give control of the car to the connection
          evnt.car.GiveControl(cn);

          BoltLog.Info("{1} entered car {0}", evnt.car, cn);
        }
      }
    } else {
      BoltLog.Error("received request to enter car which does not exist on owner");
    }
  }

  public override void SimulateController () {
    PollKeys();

    CarPlayerCommand playerCmd = BoltFactory.NewCommand<CarPlayerCommand>();
    playerCmd.input.left = left;
    playerCmd.input.right = right;
    playerCmd.input.forward = forward;
    playerCmd.input.backward = backward;
    playerCmd.input.yRotation = yRotation;

    boltEntity.QueueCommand(playerCmd);

    ClearKeys();
  }

  public override void ExecuteCommand (BoltCommand cmd, bool resetState) {
    CarPlayerCommand playerCmd = (CarPlayerCommand) cmd;

    if (resetState) {

      motor.SetState(playerCmd.state.position, playerCmd.state.velocity, playerCmd.state.grounded);

    } else {

      Vector3 movement = Vector3.zero;

      if (playerCmd.input.left) { movement.x -= 1; }
      if (playerCmd.input.right) { movement.x += 1; }
      if (playerCmd.input.forward) { movement.z += 1; }
      if (playerCmd.input.backward) { movement.z -= 1; }

      motor.transform.localRotation = Quaternion.Euler(0, playerCmd.input.yRotation, 0);
      motor.Move(motor.transform.rotation * movement);

      playerCmd.state.position = motor.position;
      playerCmd.state.velocity = motor.velocity;
      playerCmd.state.grounded = motor.grounded;

      if (playerCmd.isFirstExecution) {
        float x = 0;
        float z = 0;

        if (playerCmd.input.forward) { z = +1; }
        if (playerCmd.input.backward) { z = -1; }

        if (playerCmd.input.left) { x = -1; }
        if (playerCmd.input.right) { x = +1; }

        boltState.mecanim.MoveX = x;
        boltState.mecanim.MoveZ = z;
      }

      if (boltEntity.boltIsOwner) {
        if (motor.ground) {
          BoltEntity entity = motor.ground.FindComponent<BoltEntity>();
          if (entity) {
            CarControl cc = entity.GetComponent<CarControl>();
            if (cc) {
              boltEntity.SetOrigin(cc.transform);
            } else {
              boltEntity.SetOrigin(null);
            }
          } else {
            boltEntity.SetOrigin(null);
          }
        } else {
          boltEntity.SetOrigin(null);
        }
      }
    }
  }
}
