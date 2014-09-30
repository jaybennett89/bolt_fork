using UnityEngine;
using System.Collections;

public class CubeBehaviour : Bolt.EntityBehaviour<IPlayer> {
  public override void Attached() {
  }

  public override void SimulateOwner() {

    transform.position = (Quaternion.Euler(0, Time.time * 20f, 0) * (Vector3.forward * 10f));

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
