using UnityEngine;
using System.Collections;
using System;

[AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class PropertyEditorAttribute : System.Attribute {
  public Type PropertyType {
    get;
    private set;
  }

  public PropertyEditorAttribute(Type type) {
    PropertyType = type;
  }
}