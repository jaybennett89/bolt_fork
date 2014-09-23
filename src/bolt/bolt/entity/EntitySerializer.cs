using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  public interface IEntityBehaviour {
    BoltEntity entity { get; }

    void Attached();
    void Detached();

    void SimulateOwner();
    void SimulateController();

    void ControlGained();
    void ControlLost();
  }

  public interface IEntitySerializer : IEntityBehaviour {
    void Render();

    void BeforeStep();
    void AfterStep();

    float CalculatePriority(BoltConnection connection, BitArray mask, uint skipped);
  }

  public interface IEntitySerializer<TState> : IEntitySerializer where TState : IState {
    TState state { get; }
  }
}
