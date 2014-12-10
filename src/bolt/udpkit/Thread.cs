using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace UdpKit {
  public abstract class UdpThread {
    Thread thread;

    volatile bool running;
    volatile bool stopped;

    protected Byte[] Buffer;
    protected Object Object;
    protected UdpPlatform Platform;

    public bool Running {
      get { return running && (thread.IsAlive == true); }
    }

    public bool Stopped {
      get { return stopped || (thread.IsAlive == false); }
    }

    internal UdpThread(UdpPlatform platform) {
      Platform = platform;
      Buffer = new byte[1024];
    }

    public virtual void Start(object obj) {
      Object = obj;

      thread = new Thread(ThreadLoop);
      thread.Name = "UdpKit NAT Thread";
      thread.IsBackground = true;
      thread.Start();
    }

    public virtual void Stop() {
      stopped = false;
      running = false;
    }

    void ThreadLoop() {
      Init();

      while (running) {
        Update();
      }

      stopped = true;
    }

    void Init() {
      try {
        running = true;
        stopped = false;

        OnInit();
      }
      catch (Exception exn) { LogException(exn); }
    }

    void Update() {
      try {
        OnUpdate();
      }
      catch (Exception exn) { LogException(exn); }
    }

    void LogException(Exception exn) {
      UdpLog.Error(exn.Message);
      UdpLog.Error(exn.StackTrace);
    }

    protected byte[] GetBuffer() {
      // always zero out buffer
      Array.Clear(Buffer, 0, Buffer.Length);

      // return buffer
      return Buffer;
    }

    protected void Shutdown(string error, params object[] args) {
      // log shutdown error
      UdpLog.Error(error, args);

      // stop thread
      Stop();
    }

    protected abstract void OnInit();
    protected abstract void OnUpdate();
  }
}
