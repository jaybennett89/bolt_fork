using UnityEngine;
using System.Collections;

public class CarControl : BoltEntityBehaviour<CarSerializer, ICarState> {
  static byte carIdCounter = 0;

  CarController cc;

  public override void Attached () {
    cc = GetComponent<CarController>();

    if (boltEntity.boltIsOwner) {
      boltState.carId = ++carIdCounter;
    }
  }

  public override void SimulateOwner () {
    if (boltEntity.boltIsRemoteControlled == false) {
      cc.Move(0, 0);
    }
  }

  public override void SimulateController () {
    CarCommand cmd = BoltFactory.NewCommand<CarCommand>();

    cmd.input.steering = Input.GetAxis("Horizontal");
    cmd.input.acceleration = Input.GetAxis("Vertical");

    if (Input.GetKeyDown(KeyCode.Q)) {
      cmd.input.headlights = true;
    }

    if (Input.GetKeyDown(KeyCode.E)) {
      boltEntity.Raise(BoltFactory.NewEvent<ICarLeaveCar>());
    }

    boltEntity.QueueCommand(cmd);
  }

  public override void OnEvent (ICarLeaveCar evnt, BoltConnection cn) {
    if (ReferenceEquals(boltEntity.boltRemoteController, cn)) {
      CarConnectionToken token = (CarConnectionToken) cn.userToken;

      // get out on the left side
      Vector3 charPos = token.car.transform.TransformPoint(new Vector3(-2, 0, 0));

      // revoke control of car
      token.car.RevokeControl();
      token.car = null;

      // give back control of character, and make it active
      token.character.transform.position = charPos;
      token.character.GetComponent<CarPlayerSerializer>().boltState.isActiveInWorld = true;
      token.character.GiveControl(cn);
    }
  }

  public override void ExecuteCommand (BoltCommand cmd, bool resetState) {
    if (boltEntity.boltIsOwner) {
      CarCommand carCmd = (CarCommand) cmd;

      // move car
      cc.Move(carCmd.input.steering, carCmd.input.acceleration);

      // flip headlights
      if (carCmd.input.headlights) {
        boltState.headlights = !boltState.headlights;
      }
    }
  }
}
