﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  public interface IEntityBehaviour {
    void Attached();
    void Detached();

    void SimulateOwner();
    void SimulateController();

    void ControlGained();
    void ControlLost();

    void ExecuteCommand(BoltCommand command, bool resetState);
  }

  public interface IEntityBehaviour<TState> : IEntityBehaviour where TState : IState {
    TState state { get; }
  }

  internal interface IEntitySerializer {
    TypeId TypeId { get; }

    void OnRender();
    void OnInitialized();
    void OnCreated(EntityObject entity);
    void OnPrepareSend(BoltDoubleList<EntityProxy> proxies);

    void OnSimulateBefore();
    void OnSimulateAfter();

    float CalculatePriority(BoltConnection connection, BitArray mask, int skipped);

    BitArray GetDefaultMask();
    BitArray GetFilter(BoltConnection connection, EntityProxy proxy);

    void InitProxy(EntityProxy p);

    int Pack(BoltConnection connection, UdpStream stream, EntityProxyEnvelope proxy);
    void Read(BoltConnection connection, UdpStream stream, int frame);
  }

  internal interface IEntitySerializer<TState> : IEntitySerializer where TState : IState {
    TState state { get; }
  }

  internal interface IEntitySerializerFactory {
    Type TypeObject { get; }
    TypeId TypeId { get; }
    IEntitySerializer Create();
  }
}
