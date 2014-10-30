using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using IO = System.IO;
using SYS = System;
using UE = UnityEngine;

public static class BoltLog {
  public interface IWriter : IDisposable {
    void Info(string message);
    void Debug(string message);
    void Warn(string message);
    void Error(string message);
  }

  public class File : IWriter {
    volatile bool running = true;

    Thread thread;
    AutoResetEvent threadEvent;
    Queue<string> threadQueue;

    public File() {
      threadEvent = new AutoResetEvent(false);
      threadQueue = new Queue<string>(1024);

      thread = new Thread(WriteLoop);
      thread.IsBackground = true;
      thread.Start();
    }

    void Queue(string message) {
      lock (threadQueue) {
        threadQueue.Enqueue(message);
        threadEvent.Set();
      }
    }

    public void Info(string message) {
      Queue(message);
    }

    public void Debug(string message) {
      Queue(message);
    }

    public void Warn(string message) {
      Queue(message);
    }

    public void Error(string message) {
      Queue(message);
    }

    public void Dispose() {
      running = false;
    }

    void WriteLoop() {
      try {
        var n = DateTime.Now;

        string logFile;
        logFile = "Bolt_Log_{0}Y-{1}M-{2}D_{3}H{4}M{5}S_{6}MS.txt";
        logFile = string.Format(logFile, n.Year, n.Month, n.Day, n.Hour, n.Minute, n.Second, n.Millisecond);

        var stream = IO.File.Open(logFile, IO.FileMode.Create);
        var streamWriter = new IO.StreamWriter(stream);

        while (running) {
          if (threadEvent.WaitOne(100)) {
            lock (threadQueue) {
              while (threadQueue.Count > 0) {
                streamWriter.WriteLine(threadQueue.Dequeue());
              }
            }
          }

          streamWriter.Flush();
          stream.Flush();
        }

        streamWriter.Flush();
        streamWriter.Close();
        streamWriter.Dispose();

        stream.Flush();
        stream.Close();
        stream.Dispose();

        threadEvent.Close();
      }
      catch (Exception exn) {
        BoltLog.Exception(exn);
      }
    }
  }

  public class Console : IWriter {
    public void Info(string message) {
      BoltConsole.Write(message, UE.Color.white);
    }

    public void Debug(string message) {
      BoltConsole.Write(message, UE.Color.green);
    }

    public void Warn(string message) {
      BoltConsole.Write(message, UE.Color.yellow);
    }

    public void Error(string message) {
      BoltConsole.Write(message, UE.Color.red);
    }

    public void Dispose() {

    }
  }

  public class SystemOut : IWriter {
    public void Info(string message) {
      SYS.Console.Out.WriteLine(message);
    }

    public void Debug(string message) {
      SYS.Console.Out.WriteLine(message);
    }

    public void Warn(string message) {
      SYS.Console.Out.WriteLine(message);
    }

    public void Error(string message) {
      SYS.Console.Error.WriteLine(message);
    }

    public void Dispose() {

    }
  }

  public class Unity : IWriter {
    public void Info(string message) {
      UE.Debug.Log(message);
    }

    public void Debug(string message) {
      UE.Debug.Log(message);
    }

    public void Warn(string message) {
      UE.Debug.LogWarning(message);
    }

    public void Error(string message) {
      UE.Debug.LogError(message);
    }

    public void Dispose() {

    }
  }

  static readonly object _lock = new object();
  static List<IWriter> _writers = new List<IWriter>();

  public static void RemoveAll() {
    lock (_lock) {
      for (int i = 0; i < _writers.Count; ++i) {
        _writers[i].Dispose();
      }

      _writers = new List<IWriter>();
    }
  }

  public static void Add<T>(T instance) where T : class, IWriter {
    lock (_lock) {
      _writers.Add(instance);
    }
  }

  public static void Info(string message) {
    lock (_lock) {
      VerifyOneWriter();

      for (int i = 0; i < _writers.Count; ++i) {
        _writers[i].Info(message);
      }
    }
  }

  public static void Info(object message) {
    Info(message.ToString());
  }

  public static void Info(string message, object arg0) {
    Info(string.Format(message, arg0));
  }

  public static void Info(string message, object arg0, object arg1) {
    Info(string.Format(message, arg0, arg1));
  }

  public static void Info(string message, object arg0, object arg1, object arg2) {
    Info(string.Format(message, arg0, arg1, arg2));
  }

  public static void Info(string message, params object[] args) {
    Info(string.Format(message, args));
  }

  [Conditional("DEBUG")]
  internal static void Debug(string message) {
    lock (_lock) {
      VerifyOneWriter();

      for (int i = 0; i < _writers.Count; ++i) {
        _writers[i].Debug(message);
      }
    }
  }

  [Conditional("DEBUG")]
  internal static void Debug(object message) {
    Debug(message.ToString());
  }

  [Conditional("DEBUG")]
  internal static void Debug(string message, object arg0) {
    Debug(string.Format(message, arg0));
  }

  [Conditional("DEBUG")]
  internal static void Debug(string message, object arg0, object arg1) {
    Debug(string.Format(message, arg0, arg1));
  }

  [Conditional("DEBUG")]
  internal static void Debug(string message, object arg0, object arg1, object arg2) {
    Debug(string.Format(message, arg0, arg1, arg2));
  }

  [Conditional("DEBUG")]
  internal static void Debug(string message, params object[] args) {
    Debug(string.Format(message, args));
  }

  static void VerifyOneWriter() {
    if (_writers.Count == 0) {
      _writers.Add(new Unity());
    }
  }

  public static void Warn(string message) {
    lock (_lock) {
      VerifyOneWriter();

      for (int i = 0; i < _writers.Count; ++i) {
        _writers[i].Warn(message);
      }
    }
  }

  public static void Warn(object message) {
    Warn(message.ToString());
  }

  public static void Warn(string message, object arg0) {
    Warn(string.Format(message, arg0));
  }

  public static void Warn(string message, object arg0, object arg1) {
    Warn(string.Format(message, arg0, arg1));
  }

  public static void Warn(string message, object arg0, object arg1, object arg2) {
    Warn(string.Format(message, arg0, arg1, arg2));
  }

  public static void Warn(string message, params object[] args) {
    Warn(string.Format(message, args));
  }

  public static void Error(string message) {
    lock (_lock) {
      VerifyOneWriter();

      for (int i = 0; i < _writers.Count; ++i) {
        _writers[i].Error(message);
      }
    }
  }

  public static void Error(object message) {
    Error(message.ToString());
  }

  public static void Error(string message, object arg0) {
    Error(string.Format(message, arg0));
  }

  public static void Error(string message, object arg0, object arg1) {
    Error(string.Format(message, arg0, arg1));
  }

  public static void Error(string message, object arg0, object arg1, object arg2) {
    Error(string.Format(message, arg0, arg1, arg2));
  }

  public static void Error(string message, params object[] args) {
    Error(string.Format(message, args));
  }

  public static void Exception(Exception exception) {
    lock (_lock) {
      for (int i = 0; i < _writers.Count; ++i) {
        _writers[i].Error(exception.GetType() + ": " + exception.Message);
        _writers[i].Error(exception.StackTrace);
      }
    }
  }
}
