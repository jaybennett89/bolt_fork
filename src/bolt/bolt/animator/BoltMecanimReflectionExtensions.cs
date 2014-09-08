using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class BoltMecanimReflectionExtensions {
  struct ReflectionCache {
    public PropertyInfo stateGetter;
    public PropertyInfo mecanimGetter;
    public Dictionary<string, PropertyInfo> propertySetters;
    public Dictionary<string, MethodInfo> triggerMethods;
  }

  static readonly object[] zeroObjectArray = new object[0];
  static readonly object[] oneObjectArray = new object[1];

  static readonly Dictionary<Type, ReflectionCache> reflectionCache = new Dictionary<Type, ReflectionCache>();

  static bool boltIsRunning {
    get { return BoltCore.isServer || BoltCore.isClient; }
  }

  static ReflectionCache GetReflectionCache (Animator animator) {
    var cache = new ReflectionCache();
    var serializer = animator.GetBoltSerializer();
    var serializerType = serializer.GetType();

    if (reflectionCache.TryGetValue(serializerType, out cache) == false) {
      cache.stateGetter = serializerType.GetProperty("boltState", BindingFlags.Public | BindingFlags.Instance);
      cache.mecanimGetter = cache.stateGetter.PropertyType.GetProperty("mecanim", BindingFlags.Public | BindingFlags.Instance);
      cache.propertySetters = new Dictionary<string, PropertyInfo>();
      cache.triggerMethods = new Dictionary<string, MethodInfo>();

      foreach (var property in cache.mecanimGetter.PropertyType.GetProperties()) {
        cache.propertySetters.Add(property.Name, property);
      }

      foreach (var method in cache.mecanimGetter.PropertyType.GetMethods()) {
        cache.triggerMethods.Add(method.Name, method);
      }

      reflectionCache.Add(serializerType, cache);
    }

    return cache;
  }

  public static void SetFloatReflected (this Animator animator, string name, float value) {
    if (boltIsRunning) {
      SetValueReflected(animator, name, value, zeroObjectArray);
    } else {
      animator.SetFloat(name, value);
    }
  }

  public static void SetIntegerReflected (this Animator animator, string name, int value) {
    if (boltIsRunning) {
      SetValueReflected(animator, name, value, zeroObjectArray);
    } else {
      animator.SetInteger(name, value);
    }
  }

  public static void SetBoolReflected (this Animator animator, string name, bool value) {
    if (boltIsRunning) {
      SetValueReflected(animator, name, value, zeroObjectArray);
    } else {
      animator.SetBool(name, value);
    }
  }

  public static void SetLayerWeightReflected (this Animator animator, int layer, float weight) {
    if (boltIsRunning) {
      oneObjectArray[0] = layer;
      SetValueReflected(animator, "Item", weight, oneObjectArray);
    } else {
      animator.SetLayerWeight(layer, weight);
    }
  }

  public static void SetTriggerReflected (this Animator animator, string name) {
    if (boltIsRunning) {
      var cache = GetReflectionCache(animator);
      var method = default(MethodInfo);

      if (cache.triggerMethods.TryGetValue(name, out method)) {
        var serializer = animator.GetBoltSerializer();
        var state = cache.stateGetter.GetValue(serializer, zeroObjectArray);
        var mecanim = cache.mecanimGetter.GetValue(state, zeroObjectArray);
        method.Invoke(mecanim, zeroObjectArray);

      } else {
        BoltLog.Error("could not find a mecanim trigger named {0} on {1}", name, cache.stateGetter.PropertyType);
      }

    } else {
      animator.SetTrigger(name);
    }
  }

  static void SetValueReflected (Animator animator, string name, object value, object[] indexers) {
    var cache = GetReflectionCache(animator);
    var property = default(PropertyInfo);

    if (cache.propertySetters.TryGetValue(name, out property)) {
      var serializer = animator.GetBoltSerializer();
      var state = cache.stateGetter.GetValue(serializer, zeroObjectArray);
      var mecanim = cache.mecanimGetter.GetValue(state, zeroObjectArray);
      property.SetValue(mecanim, value, indexers);

    } else {
      BoltLog.Error("could not find a mecanim property named {0} on {1}", name, cache.stateGetter.PropertyType);
    }
  }
}
