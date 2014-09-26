using System;
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

    public override void FindAllProperties(List<StateProperty> all, int filterMask, bool controller) {
      Struct.FindAllProperties(all, (filterMask & Definition.Filters), (Definition.Controller && controller));
    }
  }
}
