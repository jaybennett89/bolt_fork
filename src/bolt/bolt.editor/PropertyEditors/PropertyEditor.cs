using UnityEngine;
using System.Collections;
using Bolt.Compiler;
using System;
using System.Collections.Generic;
using System.Reflection;

public abstract class PropertyEditor {
  public AssetDefinition Asset;
  public PropertyDefinition Definition;
  public PropertyType PropertyType;

  public void Edit(AssetDefinition asset, PropertyDefinition definition) {
    Asset = asset;
    Definition = definition;
    PropertyType = definition.PropertyType;
    Edit(false);
  }

  public void EditArrayElement(AssetDefinition asset, PropertyDefinition definition, PropertyType type) {
    Asset = asset;
    Definition = definition;
    PropertyType = type;
    Edit(true);
  }

  protected abstract void Edit(bool array);
}

public abstract class PropertyEditor<T> : PropertyEditor where T : PropertyType {
  public new T PropertyType { get { return (T)base.PropertyType; } }
}

public static class PropertyEditorRegistry {
  static Dictionary<Type, Type> editorLookup;

  public static PropertyEditor GetEditor(PropertyType propertyType) {
    return GetEditor(propertyType.GetType());
  }

  public static PropertyEditor GetEditor(Type propertyType) {
    if (editorLookup == null) {
      editorLookup = new Dictionary<Type, Type>();

      foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
        if (asm.GetName().Name.Contains("CSharp")) {
          foreach (Type type in asm.GetTypes()) {
            if (typeof(PropertyEditor).IsAssignableFrom(type) && !type.IsAbstract) {
              editorLookup.Add(type.BaseType.GetGenericArguments()[0], type); 
            }
          }
        }
      }
    }

    return (PropertyEditor)Activator.CreateInstance(editorLookup[propertyType]);
  }
}