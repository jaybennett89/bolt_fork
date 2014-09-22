using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyDecoratorStruct : PropertyDecorator<PropertyTypeStruct> {
    public StructDecorator Struct {
      get { return Generator.FindStruct(PropertyType.StructGuid); }
    }

    public override int StructCount {
      get { return Struct.StructCount; }
    }

    public override int ByteSize {
      get {
        // make sure we actually calculated the byte size for this struct
        Assert.True(Struct.ByteSizeCalculated);

        // return value
        return Struct.ByteSize;
      }
    }

    public override string ClrType {
      get { return Struct.Name; }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterStruct();
    }

    public override void GetStructList(List<StructDecorator> list) {
      Struct.GetStructList(list);
    }
  }
}
