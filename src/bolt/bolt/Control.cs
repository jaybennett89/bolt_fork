using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UdpKit;
using UnityEngine;

namespace Bolt {

  enum ControlState {
    Pending,
    Started,
    Finished
  }

  abstract class ControlCommand {
    public ControlState State;
    public int PendingFrames;

    public ManualResetEvent FinishedEvent;
    public int FinishedFrames;

    public ControlCommand() {
      State = ControlState.Pending;
      FinishedEvent = new ManualResetEvent(false);
      PendingFrames = 2;
      FinishedFrames = 2;
    }

    public abstract void Run();
    public abstract void Done(bool othersPending);
  }

  class ControlCommandStart : ControlCommand {
    public BoltConfig Config;
    public BoltNetworkModes Mode;

    public UdpPlatform Platform;
    public UdpEndPoint EndPoint;

    public override void Run() {
      BoltCore.BeginStart(FinishedEvent, Mode, EndPoint, Platform, Config);
    }

    public override void Done(bool othersPending) {

    }
  }

  class ControlCommandShutdown : ControlCommand {
    public override void Run() {
      BoltCore.BeginShutdown();
    }

    public override void Done(bool othersPending) {

    }
  }

  public class ControlBehaviour : MonoBehaviour {
    Queue<ControlCommand> commands = new Queue<ControlCommand>();

    void Start(ControlCommandStart start) {
      commands.Enqueue(start);
    }

    void Shutdown(ControlCommandShutdown shutdown) {
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
              catch { }

              cmd.State = ControlState.Started;
            }
            break;

          case ControlState.Started:
            if (cmd.FinishedEvent.WaitOne(0)) {
              cmd.State = ControlState.Finished;
            }
            break;

          case ControlState.Finished:
            if (--cmd.FinishedFrames < 0) {
              // we are done
              commands.Dequeue();

              try {
                cmd.Done(commands.Count > 0);
              }
              catch { }
            }
            break;
        }
      }
    }
  }
}
