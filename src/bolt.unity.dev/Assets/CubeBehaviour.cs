using UnityEngine;
using System.Collections;

public class CubeBehaviour : Bolt.EntityBehaviour<IPlayer> {
  public override void Attached() {
    state.SetAnimator(GetComponent<Animator>());
    state.Transform.Simulate = transform;
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
