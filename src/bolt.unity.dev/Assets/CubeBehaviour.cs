using UnityEngine;
using System.Collections;

public class CubeBehaviour : Bolt.EntityBehaviour<IPlayer> {
  public override void Attached() {
    state.Transform.Simulate = transform;
    state.Attack += Attack;
  }

  public override void SimulateOwner() {
    transform.position = new Vector3(Mathf.PingPong(Time.time * 2.5f, 10f) - 5f, 0, 0);
    //transform.position = (Quaternion.Euler(0, Time.time * 20f, 0) * (Vector3.forward * 10f));

    if ((BoltNetwork.frame % 60) == 0) {
      state.Modify().Attack();
    }
  }

  void Attack() {
    BoltLog.Info("ATTACK TRIGGERED:" + entity.GetInstanceID());
  }

  void TransformChanged(Bolt.IState character, string propertyPath, int[] propertyIndices) {
    //BoltLog.Info("Transform Changed");
  }

  //void Bolt.IEntityBehaviour.Attached() {
  //  
  //}

  //void Bolt.IEntityBehaviour.ControlGained() {
  //}

  //void Bolt.IEntityBehaviour.ControlLost() {
  //}

  //void Bolt.IEntityBehaviour.Detached() {
  //}

  //void Bolt.IEntityBehaviour.ExecuteCommand(BoltCommand command, bool resetState) {
  //}

  //void Bolt.IEntityBehaviour.SimulateController() {
  //}

  //void Bolt.IEntityBehaviour.SimulateOwner() {
  //  using (var m = GetComponent<BoltEntity>().GetState<IPlayer>().Modify()) {
  //    m.Health += 1;
  //  }
  //}
}
