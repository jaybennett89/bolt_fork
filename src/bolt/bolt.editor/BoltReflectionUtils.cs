using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

static class BoltReflectionUtils {
  public static IEnumerable<Type> FindInterfacesInNamespace (Assembly[] asms, string @namespace) {
    foreach (Assembly asm in asms) {
      foreach (Type type in asm.GetTypes()) {
        if (type.IsInterface && type.Namespace == @namespace) {
          yield return type;
        }
      }
    }
  }

  public static bool IsExtensionMethodClass (this Type type) {
    return
      type.IsClass &&
      type.IsAbstract &&
      type.IsSealed &&
      type.IsPublic &&
      type.IsVisible && 
      !type.IsNested &&
      !type.IsGenericType &&
      !type.IsGenericTypeDefinition &&
      !type.IsValueType &&
      !type.IsInterface;
  }

  public static IEnumerable<MethodInfo> FindExtensionMethods (this Type t, Assembly[] asms) {
    foreach (Assembly asm in asms) {
      foreach (Type type in asm.GetTypes()) {
        if (type.IsExtensionMethodClass()) {
          foreach (MethodInfo m in type.GetMethods(BindingFlags.Public | BindingFlags.Static)) {
            if (m.IsExtensionMethodFor(t)) {
              yield return m;
            }
          }
        }
      }
    }
  }

  public static IEnumerable<Type> FindSubtypes (this Type t) {
    foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
      foreach (Type type in asm.GetTypes()) {
        if (type.IsSubclassOf(t))
          yield return type;
      }
    }
  }

  public static IEnumerable<Type> FindInterfaceImplementations(this Type t) {
    foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
      foreach (Type type in asm.GetTypes()) {
        if (type.IsClass && t.IsAssignableFrom(type)) {
          yield return type;
        }
      }
    }
  }


  public static bool HasPublicDefaultConstructor (this Type type) {
    return type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null) != null;
  }

  public static bool IsExtensionMethodFor (this MethodInfo method, Type type) {
    if (method.GetCustomAttributes(typeof(ExtensionAttribute), false).Length == 0) {
      return false;
    }

    var parms = method.GetParameters();
    if (parms.Length == 0)
      return false;

    return parms[0].ParameterType == type && method.IsStatic;
  }

  public static string CSharpName (this Type type) {
    return type.FullName.Replace('+', '.');
  }

  public static void SetField (this object obj, string name, object value) {
    obj.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).SetValue(obj, value);
  }

  public static void SetField<T> (string name, object value) {
    typeof(T).GetField(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, value);
  }

  public static void SetField<T> (Type t, string name, object value) {
    t.GetField(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, value);
  }

  public static T GetField<T> (this object obj, string name) {
    return (T)obj.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj);
  }

  public static TField GetField<T, TField> (string name) {
    return (TField)typeof(T).GetField(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
  }

  public static TField GetField<TField> (Type t, string name) {
    return (TField)t.GetField(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
  }

  public static void SetProperty (this object obj, string name, object value) {
    obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).SetValue(obj, value, null);
  }

  public static void SetProperty<T> (string name, object value) {
    typeof(T).GetProperty(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, value, null);
  }

  public static void SetProperty<T> (Type t, string name, object value) {
    t.GetProperty(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, value, null);
  }

  public static T GetProperty<T> (this object obj, string name) {
    return (T)obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj, null);
  }

  public static TField GetProperty<T, TField> (string name) {
    return (TField)typeof(T).GetProperty(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic).GetValue(null, null);
  }

  public static TField GetProperty<TField> (Type t, string name) {
    return (TField)t.GetProperty(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic).GetValue(null, null);
  }
}
