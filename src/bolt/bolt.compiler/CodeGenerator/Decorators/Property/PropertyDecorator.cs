using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public abstract class PropertyDecorator {
    public int Index;
    public int MaskBit;
    public int ByteOffset;

    public CodeGenerator Generator;
    public AssetDecorator DefiningAsset;
    public PropertyDefinition Definition;

    public abstract int ByteSize { get; }
    public abstract string ClrType { get; }
    public abstract PropertyCodeEmitter CreateEmitter();

    public virtual int StructCount {
      get { return 0; }
    }

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
