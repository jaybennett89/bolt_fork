using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

static class BoltReflectionUtils {
  public static IEnumerable<Type> FindInterfaceImplementations(this Type t) {
    foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
      foreach (Type type in asm.GetTypes()) {
        if (type.IsClass && t.IsAssignableFrom(type)) {
          yield return type;
        }
      }
    }
  }
}
