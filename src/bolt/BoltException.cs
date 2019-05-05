using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Base class for all bolt specific exceptions
/// </summary>
public class BoltException : Exception {
  public BoltException(string message)
    : base(message) {
  }

  public BoltException(string message, object arg0)
    : base(string.Format(message, arg0)) {
  }

  public BoltException(string message, object arg0, object arg1)
    : base(string.Format(message, arg0, arg1)) {
  }

  public BoltException(string message, object arg0, object arg1, object arg2)
    : base(string.Format(message, arg0, arg1, arg2)) {
  }

  public BoltException(string message, params object[] args)
    : base(string.Format(message, args)) {
  }
}
