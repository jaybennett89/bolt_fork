#define LOG_CONSOLE
#define LOG_UNITY

using System;
using System.Diagnostics;
using UnityEngine;

public static class BoltLog {
  static readonly System.Object _lock = new System.Object();

  public static Action<string> InfoCallback = s => {
#if LOG_CONSOLE
    BoltConsole.Write(s, Color.white);
#endif
#if LOG_UNITY
    UnityEngine.Debug.Log(s);
#endif
  };

  public static Action<string> DebugCallback = s => {
#if DEBUG
#if LOG_CONSOLE
    BoltConsole.Write(s, Color.gray);
#endif
#if LOG_UNITY
    UnityEngine.Debug.Log(s);
#endif
#endif
  };

  public static Action<string> WarningCallback = s => {
#if LOG_CONSOLE
    BoltConsole.Write(s, Color.yellow);
#endif
#if LOG_UNITY
    UnityEngine.Debug.LogWarning(s);
#endif
  };

  public static Action<string> ErrorCallback = s => {
#if LOG_CONSOLE
    BoltConsole.Write(s, Color.red);
#endif
#if LOG_UNITY
    UnityEngine.Debug.LogError(s);
#endif
  };

  public static Action<Exception> ExceptionCallback = exn => {
#if LOG_CONSOLE
    BoltConsole.Write(exn.ToString(), Color.red);
#endif
#if LOG_UNITY
    UnityEngine.Debug.LogException(exn);
#endif
  };

  public static void Info (string message) {
    lock (_lock) {
      var cb = InfoCallback;

      if (cb != null)
        cb(message);
    }
  }

  public static void Info (object message) {
    Info(message.ToString());
  }

  public static void Info (string message, object arg0) {
    Info(string.Format(message, arg0));
  }

  public static void Info (string message, object arg0, object arg1) {
    Info(string.Format(message, arg0, arg1));
  }

  public static void Info (string message, object arg0, object arg1, object arg2) {
    Info(string.Format(message, arg0, arg1, arg2));
  }

  public static void Info (string message, params object[] args) {
    Info(string.Format(message, args));
  }

  [Conditional("DEBUG")]
  internal static void Debug (string message) {
    lock (_lock) {
      var cb = DebugCallback;

      if (cb != null)
        cb(message);
    }
  }

  [Conditional("DEBUG")]
  internal static void Debug (object message) {
    Debug(message.ToString());
  }

  [Conditional("DEBUG")]
  internal static void Debug (string message, object arg0) {
    Debug(string.Format(message, arg0));
  }

  [Conditional("DEBUG")]
  internal static void Debug (string message, object arg0, object arg1) {
    Debug(string.Format(message, arg0, arg1));
  }

  [Conditional("DEBUG")]
  internal static void Debug (string message, object arg0, object arg1, object arg2) {
    Debug(string.Format(message, arg0, arg1, arg2));
  }

  [Conditional("DEBUG")]
  internal static void Debug (string message, params object[] args) {
    Debug(string.Format(message, args));
  }

  public static void Warning (string message) {
    lock (_lock) {
      var cb = WarningCallback;

      if (cb != null)
        cb(message);
    }
  }

  public static void Warning (object message) {
    Warning(message.ToString());
  }

  public static void Warning (string message, object arg0) {
    Warning(string.Format(message, arg0));
  }

  public static void Warning (string message, object arg0, object arg1) {
    Warning(string.Format(message, arg0, arg1));
  }

  public static void Warning (string message, object arg0, object arg1, object arg2) {
    Warning(string.Format(message, arg0, arg1, arg2));
  }

  public static void Warning (string message, params object[] args) {
    Warning(string.Format(message, args));
  }

  public static void Error (string message) {
    lock (_lock) {
      var cb = ErrorCallback;

      if (cb != null)
        cb(message);
    }
  }

  public static void Error (object message) {
    Error(message.ToString());
  }

  public static void Error (string message, object arg0) {
    Error(string.Format(message, arg0));
  }

  public static void Error (string message, object arg0, object arg1) {
    Error(string.Format(message, arg0, arg1));
  }

  public static void Error (string message, object arg0, object arg1, object arg2) {
    Error(string.Format(message, arg0, arg1, arg2));
  }

  public static void Error (string message, params object[] args) {
    Error(string.Format(message, args));
  }

  public static void Exception (Exception exn) {
    lock (_lock) {
      var cb = ExceptionCallback;

      if (cb != null)
        cb(exn);
    }
  }
}
