using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public abstract class PropertyDecorator {
    public int ByteOffset;
    public int ObjectOffset;

    public CodeGenerator Generator;
    public AssetDecorator DefiningAsset;
    public PropertyDefinition Definition;

    public abstract string ClrType {
      get;
    }

    public abstract int ByteSize {
      get;
    }

    public virtual int ObjectSize {
      get {
        if ((DefiningAsset is StateDecorator) || (DefiningAsset is StructDecorator)) {
          if (Definition.StateAssetSettings.Callback && Definition.PropertyType.CallbackAllowed) {
            return 1;
          }
        }

        return 0;
      }
    }

    public abstract PropertyCodeEmitter CreateEmitter();

    public virtual void GetStructList(List<StructDecorator> list) {

    }

    public static List<PropertyDecorator> Decorate(IEnumerable<PropertyDefinition> definitions, AssetDecorator asset) {
      return definitions.Select(p => Decorate(p, asset)).ToList();
    }

    public static PropertyDecorator Decorate(PropertyDefinition definition, AssetDecorator asset) {
      PropertyDecorator decorator;

      decorator = definition.PropertyType.CreateDecorator();
      decorator.Generator = asset.Generator;
      decorator.Definition = definition;
      decorator.DefiningAsset = asset;

      return decorator;
    }
  }

  public abstract class PropertyDecorator<T> : PropertyDecorator where T : PropertyType {
    public T PropertyType { get { return (T)Definition.PropertyType; } }
  }
}
