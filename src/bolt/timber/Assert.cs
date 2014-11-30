using System.Diagnostics;

namespace Timber {
  public static class Assert {
    [Conditional("DEBUG")]
    internal static void Fail() {
      Fail("");
    }

    [Conditional("DEBUG")]
    internal static void Fail(string message) {
      throw new AssertFailedException("Assert.Fail", message);
    }

    [Conditional("DEBUG")]
    internal static void True(bool condition) {
      True(condition, "");
    }

    [Conditional("DEBUG")]
    internal static void True(bool condition, string message) {
      if (condition == false) {
        throw new AssertFailedException("Assert.True", message);
      }
    }

    [Conditional("DEBUG")]
    internal static void False(bool condition) {
      False(condition, "");
    }

    [Conditional("DEBUG")]
    internal static void False(bool condition, string message) {
      if (condition) {
        throw new AssertFailedException("Assert.False", message);
      }
    }

    [Conditional("DEBUG")]
    internal static void Null(object obj) {
      Null(obj, "");
    }

    [Conditional("DEBUG")]
    internal static void Null(object obj, string message) {
      if (ReferenceEquals(obj, null) == false) {
        throw new AssertFailedException("Assert.Null", message);
      }
    }

    [Conditional("DEBUG")]
    internal static void NotNull(object obj) {
      Null(obj, "");
    }

    [Conditional("DEBUG")]
    internal static void NotNull(object obj, string message) {
      if (ReferenceEquals(obj, null)) {
        throw new AssertFailedException("Assert.NotNull", message);
      }
    }

    [Conditional("DEBUG")]
    internal static void Same(object a, object b) {
      Same(a, b, "");
    }

    [Conditional("DEBUG")]
    internal static void Same(object a, object b, string message) {
      if (ReferenceEquals(a, b) == false) {
        throw new AssertFailedException("Assert.Same", message);
      }
    }

    [Conditional("DEBUG")]
    internal static void NotSame(object a, object b) {
      NotSame(a, b, "");
    }

    [Conditional("DEBUG")]
    internal static void NotSame(object a, object b, string message) {
      if (ReferenceEquals(a, b)) {
        throw new AssertFailedException("Assert.NotSame", message);
      }
    }
  }
}
