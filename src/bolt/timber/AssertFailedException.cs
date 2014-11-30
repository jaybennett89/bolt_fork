using System;

namespace Timber {
  public class AssertFailedException : Exception {
    internal AssertFailedException(string method) : base(method) { }
    internal AssertFailedException(string method, string msg) : base(method + ": " + msg) { }
  }
}
