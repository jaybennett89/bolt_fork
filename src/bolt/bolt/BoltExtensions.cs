using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BoltExtensions {

  public static bool Has<T> (this T[] array, int index) where T : class {
    return index < array.Length && array[index] != null;
  }

  public static bool Has<T> (this T[] array, uint index) where T : class {
    return index < array.Length && array[index] != null;
  }

  public static bool TryGetIndex<T> (this T[] array, int index, out T value) where T : class {
    if (index < array.Length)
      return (value = array[index]) != null;

    value = default(T);
    return false;
  }

  public static bool TryGetIndex<T> (this T[] array, uint index, out T value) where T : class {
    if (index < array.Length)
      return (value = array[index]) != null;

    value = default(T);
    return false;
  }

  public static T FindComponent<T> (this Component component) where T : Component {
    return FindComponent<T>(component.transform);
  }

  public static T FindComponent<T> (this GameObject gameObject) where T : Component {
    return FindComponent<T>(gameObject.transform);
  }

  public static T FindComponent<T> (this Transform transform) where T : Component {
    T component = null;

    while (transform && !component) {
      component = transform.GetComponent<T>();
      transform = transform.parent;
    }

    return component;
  }
}
