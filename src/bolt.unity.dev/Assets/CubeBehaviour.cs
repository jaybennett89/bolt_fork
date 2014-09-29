using UnityEngine;
using System.Collections;

public class CubeBehaviour : MonoBehaviour, Bolt.IEntityBehaviour {
  void Bolt.IEntityBehaviour.Attached() {
    GetComponent<BoltEntity>().GetState<IPlayer>().AddCallback(".Health", HealthChanged);
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

  void HealthChanged(Bolt.IState character, string path, int[] indices) {
    var en = GetComponent<BoltEntity>();
    BoltLog.Info("Health of {0}#{1} changed to {2}", en, en.GetInstanceID(), en.GetState<IPlayer>().Health);
  }
}
