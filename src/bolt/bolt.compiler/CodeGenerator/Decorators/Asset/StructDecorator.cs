﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class StructDecorator : AssetDecorator<StructDefinition> {
    public int FrameSize;
    public bool FrameSizeCalculated;

    public StateDecorator SourceState = null;
    public List<PropertyDecorator> Properties = new List<PropertyDecorator>();

    public bool BasedOnState {
      get { return SourceState != null; }
    }

    public string ArrayName {
      get { return Name + "Array"; }
    }

    public string ModifierName {
      get { return Name + "Modifier"; }
    }

    public List<StructDecorator> GetStructList(List<StructDecorator> list) {
      list.Add(this);

      for (int i = 0; i < Properties.Count; ++i) {
        Properties[i].GetStructList(list);
      }

      return list;
    }

    public override string ToString() {
      return string.Format("[Struct {0}]", Name);
    }

    public IEnumerable<StructDecorator> Dependencies {
      get {
        foreach (PropertyDecorator pd in Properties) {
          var typeStruct = pd.Definition.PropertyType as PropertyTypeStruct;
          if (typeStruct != null) {
            yield return Generator.FindStruct(typeStruct.StructGuid);

            foreach (StructDecorator sd in Generator.FindStruct(typeStruct.StructGuid).Dependencies) {
              yield return sd;
            }
          }

          var typeArray = pd.Definition.PropertyType as PropertyTypeArray;
          if (typeArray != null) {
            var typeArrayStruct = typeArray.ElementType as PropertyTypeStruct;
            if (typeArrayStruct != null) {
              yield return Generator.FindStruct(typeArrayStruct.StructGuid);

              foreach (StructDecorator sd in Generator.FindStruct(typeArrayStruct.StructGuid).Dependencies) {
                yield return sd;
              }
            }
          }
        }
      }
    }
  }
}
