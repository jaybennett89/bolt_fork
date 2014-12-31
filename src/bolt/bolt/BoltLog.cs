using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using IO = System.IO;
using SYS = System;
using UE = UnityEngine;

/// <summary>
/// Provides logging capabilities to a variety of outputs
/// </summary>
/// <example>
/// *Example:* Logging with different status levels.
/// 
/// ```csharp
/// void OwnerOnAttack(BoltEntity player, BoltEntity target) {
///   if(!target.isAttached) {
///     BoltLog.Error("Attempting to target an entity that is not attached: {0}", target.networkId);
///   }
///   else {
///     BoltLog.Info("{0} attack on {1}", player.networkId, target.networkId);
///     
///     var playerState = player.GetState&ltPlayerState&gt();
///     var targetState = target.GetState&ltPlayerState&gt();
///     
///     using (var mod = targetState.Modify()) {
///       mod.HP -= playerState.BaseDamage * playerState.DamageModMultiplier;       
///     }
///   }
/// }
/// ```
/// </example>
public static class BoltLog {

  /// <summary>
  /// The interface providing log writing capabilities to an output
  /// </summary>
  public interface IWriter : IDisposable {
    void Info(string message);
    void Debug(string message);
    void Warn(string message);
    void Error(string message);
  }

  /// <summary>
  /// IWriter implementation that outputs to a file
  /// </summary>
  public class File : IWriter {
    volatile bool running = true;

    bool isServer;
    Thread thread;
    AutoResetEvent threadEvent;
    Queue<string> threadQueue;

    public File(bool server) {
      isServer = server;
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

    void IWriter.Info(string message) {
      Queue(message);
    }

    void IWriter.Debug(string message) {
      Queue(message);
    }

    void IWriter.Warn(string message) {
      Queue(message);
    }

    void IWriter.Error(string message) {
      Queue(message);
    }

    public void Dispose() {
      running = false;
    }

    void WriteLoop() {
      try {
        var n = DateTime.Now;

        string logFile;
        logFile = "Bolt_Log_{7}_{0}Y-{1}M-{2}D_{3}H{4}M{5}S_{6}MS.txt";
        logFile = string.Format(logFile, n.Year, n.Month, n.Day, n.Hour, n.Minute, n.Second, n.Millisecond, isServer ? "SERVER" : "CLIENT");

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
        Exception(exn);
      }
    }
  }

  /// <summary>
  /// IWriter implementation that outputs to the Bolt console
  /// </summary>
  public class Console : IWriter {
    void IWriter.Info(string message) {
      BoltConsole.Write(message, BoltGUI.Sky);
    }

    void IWriter.Debug(string message) {
      BoltConsole.Write(message, BoltGUI.Green);
    }

    void IWriter.Warn(string message) {
      BoltConsole.Write(message, BoltGUI.Orange);
    }

    void IWriter.Error(string message) {
      BoltConsole.Write(message, BoltGUI.Error);
    }

    public void Dispose() {

    }
  }

  /// <summary>
  /// IWriter implementation that outputs to the system console out
  /// </summary>
  public class SystemOut : IWriter {
    void IWriter.Info(string message) {
      SYS.Console.Out.WriteLine(message);
    }

    void IWriter.Debug(string message) {
      SYS.Console.Out.WriteLine(message);
    }

    void IWriter.Warn(string message) {
      SYS.Console.Out.WriteLine(message);
    }

    void IWriter.Error(string message) {
      SYS.Console.Error.WriteLine(message);
    }

    public void Dispose() {

    }
  }

  /// <summary>
  /// IWriter implementation that outputs to Unity console
  /// </summary>
  public class Unity : IWriter {
    void IWriter.Info(string message) {
      UE.Debug.Log(message);
    }

    void IWriter.Debug(string message) {
      UE.Debug.Log(message);
    }

    void IWriter.Warn(string message) {
      UE.Debug.LogWarning(message);
    }

    void IWriter.Error(string message) {
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

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  public static void Info(string message) {
    lock (_lock) {
      VerifyOneWriter();

      for (int i = 0; i < _writers.Count; ++i) {
        _writers[i].Info(message);
      }
    }
  }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  public static void Info(object message) {
    Info(Format(message));
  }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  public static void Info(string message, object arg0) {
    Info(Format(message, arg0));
  }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  public static void Info(string message, object arg0, object arg1) {
    Info(Format(message, arg0, arg1));
  }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  public static void Info(string message, object arg0, object arg1, object arg2) {
    Info(Format(message, arg0, arg1, arg2));
  }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  public static void Info(string message, params object[] args) {
    Info(Format(message, args));
  }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  internal static void Debug(string message) {
    lock (_lock) {
      VerifyOneWriter();

      for (int i = 0; i < _writers.Count; ++i) {
        _writers[i].Debug(message);
      }
    }
  }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  internal static void Debug(object message) {
    Debug(Format(message));
  }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  internal static void Debug(string message, object arg0) {
    Debug(Format(message, arg0));
  }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  internal static void Debug(string message, object arg0, object arg1) {
    Debug(Format(message, arg0, arg1));
  }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  internal static void Debug(string message, object arg0, object arg1, object arg2) {
    Debug(Format(message, arg0, arg1, arg2));
  }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  internal static void Debug(string message, params object[] args) {
    Debug(Format(message, args));
  }

  static void VerifyOneWriter() {
    if (_writers.Count == 0) {
      //_writers.Add(new Unity());
    }
  }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  public static void Warn(string message) {
    lock (_lock) {
      VerifyOneWriter();

      for (int i = 0; i < _writers.Count; ++i) {
        _writers[i].Warn(message);
      }
    }
  }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  public static void Warn(object message) {
    Warn(Format(message));
  }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  public static void Warn(string message, object arg0) {
    Warn(Format(message, arg0));
  }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  public static void Warn(string message, object arg0, object arg1) {
    Warn(Format(message, arg0, arg1));
  }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  public static void Warn(string message, object arg0, object arg1, object arg2) {
    Warn(Format(message, arg0, arg1, arg2));
  }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  public static void Warn(string message, params object[] args) {
    Warn(Format(message, FixNulls(args)));
  }

  static object[] FixNulls(object[] args) {
    if (args == null) {
      args = new object[0];
    }

    for (int i = 0; i < args.Length; ++i) {
      if (ReferenceEquals(args[i], null)) {
        args[i] = "NULL";
      }
    }

    return args;
  }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  public static void Error(string message) {
    lock (_lock) {
      VerifyOneWriter();

      for (int i = 0; i < _writers.Count; ++i) {
        _writers[i].Error(message);
      }
    }
  }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  public static void Error(object message) {
    Error(Format(message));
  }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  public static void Error(string message, object arg0) {
    Error(Format(message, arg0));
  }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  public static void Error(string message, object arg0, object arg1) {
    Error(Format(message, arg0, arg1));
  }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  public static void Error(string message, object arg0, object arg1, object arg2) {
    Error(Format(message, arg0, arg1, arg2));
  }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  public static void Error(string message, params object[] args) {
    Error(Format(message, args));
  }

#if !DEBUG && !LOG
  [Conditional("_DISABLE_LOG_")]
#endif
  public static void Exception(Exception exception) {
    lock (_lock) {
      for (int i = 0; i < _writers.Count; ++i) {
        _writers[i].Error(exception.GetType() + ": " + exception.Message);
        _writers[i].Error(exception.StackTrace);
      }
    }
  }


  static string Format(object message) {
    return message == null ? "NULL" : message.ToString();
  }

  static string Format(string message, object arg0) {
    return string.Format(Format(message), Format(arg0));
  }

  static string Format(string message, object arg0, object arg1) {
    return string.Format(Format(message), Format(arg0), Format(arg1));
  }

  static string Format(string message, object arg0, object arg1, object arg2) {
    return string.Format(Format(message), Format(arg0), Format(arg1), Format(arg2));
  }

  static string Format(string message, object[] args) {
    if (args == null) {
      return Format(message);
    }

    for (int i = 0; i < args.Length; ++i) {
      args[i] = Format(args[i]);
    }

    return string.Format(Format(message), args);
  }
}
