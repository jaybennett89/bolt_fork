using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  public interface IEntityBehaviour {
    void Initialized();

    void Attached();
    void Detached();

    void SimulateOwner();
    void SimulateController();

    void ControlGained();
    void ControlLost();

    void MissingCommand(Bolt.Command previous);
    void ExecuteCommand(Bolt.Command command, bool resetState);
  }

  public interface IPriorityCalculator {
    float CalculateStatePriority(BoltConnection connection, BitArray mask, int skipped);
    float CalculateEventPriority(BoltConnection connection, Event evnt);
  }

  public interface IEntityBehaviour<TState> : IEntityBehaviour where TState : IState {
    TState state { get; }
  }

  internal interface IEntitySerializer {
    TypeId TypeId { get; }

    void OnRender();
    void OnInitialized();
    void OnCreated(Entity entity);
    void OnParentChanging(Entity newParent, Entity oldParent);

    void OnSimulateBefore();
    void OnSimulateAfter();

    BitArray GetDefaultMask();
    BitArray GetFilter(BoltConnection connection, EntityProxy proxy);

    void DebugInfo();
    void InitProxy(EntityProxy p);

    int Pack(BoltConnection connection, UdpStream stream, EntityProxyEnvelope proxy);
    void Read(BoltConnection connection, UdpStream stream, int frame);
  }

  internal interface IEntitySerializer<TState> : IEntitySerializer where TState : IState {
    TState state { get; }
  }
}
