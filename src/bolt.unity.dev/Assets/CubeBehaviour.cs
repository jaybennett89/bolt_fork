using UnityEngine;
using System.Collections;

public class CubeBehaviour : Bolt.EntityBehaviour<IPlayer> {
  public override void Attached() {
    state.AddCallback(".Health", HealthChanged);
  }

  public override void SimulateOwner() {
    using (var s = state.Modify()) {
      //s.Health += 1;
    }
  }

  void HealthChanged(Bolt.IState character, string propertyPath, int[] propertyIndices) {
    BoltLog.Info("Health changed to {0}", state.Health);
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
