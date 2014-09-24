using UnityEngine;
using System.Collections;
using Bolt.Compiler;
using System;
using System.Collections.Generic;
using System.Reflection;

public abstract class PropertyEditor {
  public PropertyDefinition Definition;
  public PropertyType PropertyType { get { return Definition.PropertyType; } }

  public void Edit(PropertyDefinition definition) {
    Definition = definition;
    Edit();
  }

  protected abstract void Edit();
}

public abstract class PropertyEditor<T> : PropertyEditor where T : PropertyType {
  public new T Type { get { return (T)base.PropertyType; } }
}

public static class PropertyEditorRegistry {
  static Dictionary<Type, Type> editorLookup;

  public static PropertyEditor GetEditor(Type propertyType) {
    if (editorLookup == null) {
      editorLookup = new Dictionary<Type, Type>();

      foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
        if (asm.GetName().Name.Contains("CSharp")) {
          foreach (Type type in asm.GetTypes()) {
            if (typeof(PropertyEditor).IsAssignableFrom(type) && !type.IsAbstract) {
              var attributes = type.GetCustomAttributes(typeof(PropertyEditorAttribute), false);
              if (attributes.Length == 1) {
                var attr = (PropertyEditorAttribute)attributes[0];
                editorLookup.Add(attr.PropertyType, type);
              }
            }
          }
        }
      }
    }

    return (PropertyEditor)Activator.CreateInstance(editorLookup[propertyType]);
  }
}