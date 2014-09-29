using UnityEngine;
using System.Collections;

public class CubeBehaviour : Bolt.EntityBehaviour<IPlayer> {
  public override void Attached() {
    state.AddCallback(".Health", HealthChanged);
    state.AddCallback(".Inventory[].Enchant[].Value", EnchantValueChanged);
  }

  void EnchantValueChanged(Bolt.IState character, string propertyPath, int[] propertyIndices) {
    var newValue = state.Inventory[propertyIndices[0]].Enchant[propertyIndices[1]].Value;
    BoltLog.Info("Enchant Value of Item:{0}'s Enchant:{1} changed to {0}", propertyIndices[0], propertyIndices[1], newValue);
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
