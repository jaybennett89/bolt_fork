﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyDecoratorStruct : PropertyDecorator<PropertyTypeStruct> {
    public StructDecorator Struct {
      get { return Generator.FindStruct(PropertyType.StructGuid); }
    }

    public override int ByteSize {
      get {
        // make sure we actually calculated the byte size for this struct
        Assert.True(Struct.FrameSizeCalculated);

        // return value
        return Struct.ByteSize;
      }
    }

    public override int ObjectSize {
      get {
        // make sure we actually calculated the property size for this struct
        Assert.True(Struct.FrameSizeCalculated);

        // return value
        return Struct.ObjectSize;
      }
    }

    public override string ClrType {
      get { return Struct.Name; }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterStruct();
    }

    public override void FindAllProperties(List<StateDecoratorProperty> all, StateDecoratorProperty p) {
      p.Filters = p.Filters & Definition.Filters;
      p.Controller = p.Controller && Definition.Controller;
      p.CallbackPaths = p.CallbackPaths.Add(p.CallbackPaths[p.CallbackPaths.Length - 1] + "." + Definition.Name);
      Struct.FindAllProperties(all, p);
    }
  }
}