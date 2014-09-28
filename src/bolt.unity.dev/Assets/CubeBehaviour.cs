using UnityEngine;
using System.Collections;

public class CubeBehaviour : MonoBehaviour, Bolt.IEntityBehaviour {
  void Bolt.IEntityBehaviour.Attached() {
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

  void FixedUpdate() {
    var en = GetComponent<BoltEntity>();

    if (!en.isOwner) {
      BoltLog.Info("Remote Health: {0}", en.GetState<IPlayer>().Health);
    }
  }
}
