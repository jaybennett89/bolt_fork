using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyDecoratorArray : PropertyDecorator<PropertyTypeArray> {
    public override int ByteSize {
      get { return ElementDecorator.ByteSize * PropertyType.ElementCount; }
    }

    public override int StructCount {
      get { return ElementDecorator.StructCount * PropertyType.ElementCount; }
    }

    public PropertyDecorator ElementDecorator {
      get {
        PropertyDefinition elementDefinition;

        elementDefinition = Serializer.DeepClone(Definition);
        elementDefinition.PropertyType = PropertyType.ElementType;

        return PropertyDecorator.Decorate(elementDefinition, DefiningAsset);
      }
    }

    public override string ClrType {
      get { return ElementDecorator.ClrType + "Array"; }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterArray();
    }

    public override void GetStructList(List<StructDecorator> list) {
      if (PropertyType.ElementType is PropertyTypeStruct) {
        StructDecorator s = Generator.FindStruct(((PropertyTypeStruct)PropertyType.ElementType).StructGuid);

        for (int i = 0; i < PropertyType.ElementCount; ++i) {
          s.GetStructList(list);
        }
      }
    }
  }
}
