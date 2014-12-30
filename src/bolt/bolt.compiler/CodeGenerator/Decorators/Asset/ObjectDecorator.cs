using System.Collections.Generic;

namespace Bolt.Compiler {
  public class ObjectDecorator : AssetDecorator<ObjectDefinition> {
    public override string FactoryInterface {
      get { return "Bolt.IFactory"; }
    }

    public override List<PropertyDecorator> Properties {
      get;
      set;
    }

    public override bool EmitLegacyModifyMethod {
      get { return true; }
    }

    public IEnumerable<ObjectDecorator> Dependencies {
      get {
        foreach (PropertyDecorator pd in Properties) {
          var typeStruct = pd.Definition.PropertyType as PropertyTypeObject;
          if (typeStruct != null) {
            yield return Generator.FindStruct(typeStruct.StructGuid);

            foreach (ObjectDecorator sd in Generator.FindStruct(typeStruct.StructGuid).Dependencies) {
              yield return sd;
            }
          }

          var typeArray = pd.Definition.PropertyType as PropertyTypeArray;
          if (typeArray != null) {
            var typeArrayStruct = typeArray.ElementType as PropertyTypeObject;
            if (typeArrayStruct != null) {
              yield return Generator.FindStruct(typeArrayStruct.StructGuid);

              foreach (ObjectDecorator sd in Generator.FindStruct(typeArrayStruct.StructGuid).Dependencies) {
                yield return sd;
              }
            }
          }
        }
      }
    }

    public ObjectDecorator(ObjectDefinition def) {
      Definition = def;
    }
  }
}
