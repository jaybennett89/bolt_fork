using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace UdpKit {
  public partial class NatProbe {
    abstract class Peer {
      public NatProbe Probe { get; private set; }

      public Peer(NatProbe probe) {
        Probe = probe;
      }

      public abstract void Init();
      public abstract void Update();
    }

    public const byte PROBE_COUNT = 3;

    public const byte PROBE0 = 0;
    public const byte PROBE1 = 1;
    public const byte PROBE2 = 2;

    public const byte CLIENT0 = 128;
    public const byte CLIENT1 = 129;

    Peer peer;
    Thread thread;
    UdpPlatformProvider platform;

    volatile bool running;
    volatile bool stopped;
    volatile NatProbeResult result;

    internal NatProbeConfig Config;
    public ManualResetEvent DoneEvent;

    public bool Running {
      get { return running && (thread.IsAlive == true); }
    }

    public bool Stopped {
      get { return stopped || (thread.IsAlive == false); }
    }

    public NatProbeResult Result {
      get { return result; }
    }

    public NatProbe(UdpPlatformProvider p) {
      platform = p;
      DoneEvent = new ManualResetEvent(false);
    }

    public void Start(NatProbeConfig config) {
      Config = config;

      thread = new Thread(ThreadLoop);
      thread.Name = "UdpKit NatProbe Thread";
      thread.IsBackground = true;
      thread.Start();
    }

    public void Stop() {
      stopped = false;
      running = false;

      DoneEvent.Set();
    }

    void ThreadLoop() {
      Init();

      while (running) {
        Update();
        Sleep();
      }

      stopped = true;
    }

    void Init() {
      try {
        running = true;
        stopped = false;

        switch (Config.Mode) {
          case NatProbeMode.Client: peer = new Client(this); break;
          case NatProbeMode.Server: peer = new Server(this); break;
        }

        peer.Init();
      }
      catch (Exception exn) { LogException(exn); }
    }

    void Update() {
      try {
        peer.Update();
      }
      catch (Exception exn) { LogException(exn); }
    }

    void Sleep() {
      try {
        Thread.Sleep(Config.UpdateRate);
      }
      catch (Exception exn) { LogException(exn); }
    }

    void LogException(Exception exn) {
      UdpLog.Error(exn.Message);
      UdpLog.Error(exn.StackTrace);
    }

    void Shutdown(string error, params object[] args) {
      // log shutdown error
      UdpLog.Error(error, args);

      // set result to failed
      result = NatProbeResult.Failed;

      // stop probe thread
      Stop();
    }
  }
}
