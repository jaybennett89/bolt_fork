using System;
using System.Collections.Generic;
using System.Threading;
using UdpKit;
using UnityEngine;

namespace Bolt {
  public delegate void AddCallback(Action callbackAction);

  enum ControlState {
    Pending,
    Started,
    Failed,
    Finished
  }

  abstract class ControlCommand {
    public int PendingFrames;
    public int FinishedFrames;

    public ControlState State;
    public ManualResetEvent FinishedEvent;

    public ControlCommand() {
      State = ControlState.Pending;
      FinishedEvent = new ManualResetEvent(false);
      PendingFrames = 2;
      FinishedFrames = 2;
    }

    public abstract void Run();
    public abstract void Done();
  }

  class ControlCommandStart : ControlCommand {
    public BoltConfig Config;
    public BoltNetworkModes Mode;

    public UdpPlatform Platform;
    public UdpEndPoint EndPoint;

    public override void Run() {
      BoltCore.BeginStart(this);
    }

    public override void Done() {

    }
  }

  class ControlCommandShutdown : ControlCommand {
    public List<Action> Callbacks = new List<Action>();

    public override void Run() {
      BoltCore.BeginShutdown(this);
    }

    public override void Done() {
      BoltCore._mode = BoltNetworkModes.None;

      for (int i = 0; i < Callbacks.Count; ++i) {
        try {
          Callbacks[i]();
        }
        catch (Exception exn) {
          Debug.LogException(exn);
        }
      }
    }
  }

  public class ControlBehaviour : MonoBehaviour {
    Queue<ControlCommand> commands = new Queue<ControlCommand>();

    void QueueStart(ControlCommandStart start) {
      commands.Enqueue(start);
    }

    void QueueShutdown(ControlCommandShutdown shutdown) {
      commands.Enqueue(shutdown);
    }

    void Update() {
      if (commands.Count > 0) {
        var cmd = commands.Peek();

        switch (cmd.State) {
          case ControlState.Pending:
            if (--cmd.PendingFrames < 0) {
              try {
                cmd.Run();
              }
              catch (Exception exn) {
                Debug.LogException(exn);
              }

              cmd.State = ControlState.Started;
            }
            break;

          case ControlState.Started:
            if (cmd.FinishedEvent.WaitOne(0)) {
              cmd.State = ControlState.Finished;
            }
            break;

          case ControlState.Failed:
            commands.Clear();
            break;

          case ControlState.Finished:
            if (--cmd.FinishedFrames < 0) {
              // we are done
              commands.Dequeue();

              try {
                cmd.Done();
              }
              catch (Exception exn) {
                Debug.LogException(exn);
              }
            }
            break;
        }
      }
    }
  }
}
