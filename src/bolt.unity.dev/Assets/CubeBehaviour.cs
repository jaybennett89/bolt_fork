using UnityEngine;
using System.Collections;

public class CubeBehaviour : MonoBehaviour, Bolt.IEntityBehaviour {
  void Bolt.IEntityBehaviour.Attached() {
    GetComponent<BoltEntity>().GetState<IPlayer>().HealthChanged += HealthChanged;
  }

  void Bolt.IEntityBehaviour.ControlGained() {
  }

  void Bolt.IEntityBehaviour.ControlLost() {
  }

  void Bolt.IEntityBehaviour.Detached() {
  }

  void Bolt.IEntityBehaviour.ExecuteCommand(BoltCommand command, bool resetState) {
  }

  void Bolt.IEntityBehaviour.SimulateController() {
  }

  void Bolt.IEntityBehaviour.SimulateOwner() {
    using (var m = GetComponent<BoltEntity>().GetState<IPlayer>().Modify()) {
      m.Health += 1;
    }
  }

  void HealthChanged(ICharacter character) {
    var en = GetComponent<BoltEntity>();
    BoltLog.Info("Health Of {0} is {1}", en, en.GetState<IPlayer>().Health);
  }
}
