﻿using System;
using System.Diagnostics;

public class BoltAssertFailedException : Exception {
  internal BoltAssertFailedException () { }
  internal BoltAssertFailedException (string msg) : base(msg) { }
}

internal static class Assert {
  [Conditional("DEBUG")]
  internal static void Fail () {
    throw new BoltAssertFailedException();
  }

  [Conditional("DEBUG")]
  internal static void Fail (string message) {
    throw new BoltAssertFailedException(message);
  }

  [Conditional("DEBUG")]
  internal static void Same (object a, object b) {
    NotNull(a);
    NotNull(b);
    True(ReferenceEquals(a, b));
  }

  [Conditional("DEBUG")]
  internal static void NotSame (object a, object b) {
    NotNull(a);
    NotNull(b);
    False(ReferenceEquals(a, b));
  }

  [Conditional("DEBUG")]
  internal static void Null (object a) {
    True(ReferenceEquals(a, null), "object was not null");
  }

  [Conditional("DEBUG")]
  internal static void NotNull (object a) {
    False(ReferenceEquals(a, null), "object was null");
  }

  [Conditional("DEBUG")]
  internal static void Equal (object a, object b) {
    NotNull(a);
    NotNull(b);
    True(a.Equals(b));
  }

  [Conditional("DEBUG")]
  internal static void Equal<T> (T a, T b) where T : IEquatable<T> {
    True(a.Equals(b));
  }

  [Conditional("DEBUG")]
  internal static void NotEqual (object a, object b) {
    NotNull(a);
    NotNull(b);
    False(a.Equals(b));
  }

  [Conditional("DEBUG")]
  internal static void NotEqual<T> (T a, T b) where T : IEquatable<T> {
    False(a.Equals(b));
  }

  [Conditional("DEBUG")]
  internal static void True (bool condition) {
    if (!condition) {
      throw new BoltAssertFailedException();
    }
  }

  [Conditional("DEBUG")]
  internal static void False (bool condition) {
    if (condition) {
      throw new BoltAssertFailedException();
    }
  }


  [Conditional("DEBUG")]
  internal static void False (bool condition, string message) {
    if (condition) {
      throw new BoltAssertFailedException(message);
    }
  }

  [Conditional("DEBUG")]
  internal static void True (bool condition, string message) {
    if (!condition) {
      throw new BoltAssertFailedException(message);
    }
  }

  [Conditional("DEBUG")]
  internal static void True (bool condition, string message, params object[] args) {
    if (!condition) {
      throw new BoltAssertFailedException(String.Format(message, args));
    }
  }
}

