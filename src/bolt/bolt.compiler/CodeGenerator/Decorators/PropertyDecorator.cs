using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyDecorator {
    public int Bit;
    public int Index;
    public int ByteOffset;
    public CodeGenerator Generator;
    public PropertyDefinition Definition;
    public AssetDecorator DefiningAsset;

    public CodeTypeReference PropertyTypeReference {
      get { return new CodeTypeReference(Definition.PropertyType.UserType); }
    }

    public static List<PropertyDecorator> Decorate(IEnumerable<PropertyDefinition> definitions, AssetDecorator asset) {
      return
        definitions
          .Select(x => new PropertyDecorator {
            Definition = x,
            Generator = asset.Generator,
            DefiningAsset = asset
          })
          .ToList();
    }
  }
}
