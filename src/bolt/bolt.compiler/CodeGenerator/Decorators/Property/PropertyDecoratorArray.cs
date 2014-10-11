using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyDecoratorArray : PropertyDecorator<PropertyTypeArray> {
    public override int ByteSize {
      get {
        return ElementDecorator.ByteSize * PropertyType.ElementCount;
      }
    }

    public override int ObjectSize {
      get {
        return ElementDecorator.ObjectSize * PropertyType.ElementCount;
      }
    }

    public PropertyDecorator ElementDecorator {
      get {
        PropertyDefinition elementDefinition;

        elementDefinition = Serializer.DeepClone(Definition);
        elementDefinition.IsArrayElement = true;
        elementDefinition.PropertyType = PropertyType.ElementType;

        return PropertyDecorator.Decorate(elementDefinition, DefiningAsset);
      }
    }

    public override string ClrType {
      get {
        if (ElementDecorator is PropertyDecoratorStruct) {
          return ElementDecorator.ClrType + "Array";
        }

        return ElementDecorator.GetType().Name.Replace("PropertyDecorator", "") + "Array";
      }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterArray();
    }

    public override void FindAllProperties(List<StateProperty> all, StateProperty p) {
      var structType = PropertyType.ElementType as PropertyTypeStruct;
      var structDec = default(StructDecorator);

      if ((structType != null) && (structType.StructGuid != Guid.Empty)) {
        structDec = Generator.FindStruct(structType.StructGuid);

        if (structDec == null) {
          return;
        }
      }

      for (int i = 0; i < PropertyType.ElementCount; ++i) {
        StateProperty elementProperty =
          p.Combine(Definition.Filters, Definition.Controller).AddCallbackPath(Definition.Name + "[]").AddIndex(i);

        if (structDec != null) {
          structDec.FindAllProperties(all, elementProperty);
        }
        else {
          ElementDecorator.FindAllProperties(all, elementProperty);
        }
      }
    }
  }
}
