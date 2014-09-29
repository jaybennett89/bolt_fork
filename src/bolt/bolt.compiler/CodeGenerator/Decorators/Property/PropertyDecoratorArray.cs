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

    public override void FindAllProperties(List<StateDecoratorProperty> all, StateDecoratorProperty p) {
      var structType = PropertyType.ElementType as PropertyTypeStruct;

      if ((structType != null) && (structType.StructGuid != Guid.Empty)) {
        StructDecorator dec = Generator.FindStruct(structType.StructGuid);

        for (int i = 0; i < PropertyType.ElementCount; ++i) {
          StateDecoratorProperty p_ = p;

          p_.Filters = Definition.Filters & p.Filters;
          p_.Controller = Definition.Controller && p.Controller;
          p_.CallbackPaths = p.CallbackPaths.Add(p.CallbackPaths[p.CallbackPaths.Length - 1] + "." + Definition.Name + "[]");
          p_.CallbackIndices = p.CallbackIndices.Add(i);

          dec.FindAllProperties(all, p_);
        }
      }
    }
  }
}
