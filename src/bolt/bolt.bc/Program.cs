using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bolt.Compiler;

namespace Bolt.Bc {
  class Program {
    static void Main(string[] args) {
      StructDefinition baz = new StructDefinition();
      baz.AssetPath = "Test/Baz.asset";
      baz.Comment = "My Comment";
      baz.Enabled = true;
      baz.Deleted = false;
      baz.Guid = Guid.NewGuid();
      baz.Properties = new List<PropertyDefinition>();
      baz.Properties.Add(new PropertyDefinition {
        Comment = "My Comment",
        Deleted = false,
        Enabled = true,
        Expanded = false,
        Name = "TestFloat_Baz",
        Replicated = true,
        PropertyType = new PropertyTypeFloat(),
        AssetSettings = new PropertyDefinitionStateAssetSettings(),
      });

      StateDefinition foo = new StateDefinition();
      foo.AssetPath = "Test/Foo.asset";
      foo.Comment = "My Comment";
      foo.Enabled = true;
      foo.Deleted = false;
      foo.Guid = Guid.NewGuid();
      foo.IsAbstract = false;
      foo.Properties = new List<PropertyDefinition>();

      foo.Properties.Add(new PropertyDefinition {
        Comment = "My Comment",
        Deleted = false,
        Enabled = true,
        Expanded = false,
        Name = "TestFloat",
        Replicated = true,
        PropertyType = new PropertyTypeFloat(),
        AssetSettings = new PropertyDefinitionStateAssetSettings(),
      });

      StateDefinition bar = new StateDefinition();
      bar.AssetPath = "Test/Bar.asset";
      bar.Comment = "My Comment";
      bar.Enabled = true;
      bar.Deleted = false;
      bar.Guid = Guid.NewGuid();
      bar.ParentGuid = foo.Guid;
      bar.IsAbstract = false;
      bar.Properties = new List<PropertyDefinition>();

      bar.Properties.Add(new PropertyDefinition {
        Comment = "My Comment",
        Deleted = false,
        Enabled = true,
        Expanded = false,
        Name = "TestString",
        Replicated = true,
        PropertyType = new PropertyTypeString(),
        AssetSettings = new PropertyDefinitionStateAssetSettings(),
      });

      bar.Properties.Add(new PropertyDefinition {
        Comment = "My Comment",
        Deleted = false,
        Enabled = true,
        Expanded = false,
        Name = "TestStruct",
        Replicated = true,
        PropertyType = new PropertyTypeStruct {
          StructGuid = baz.Guid
        },
        AssetSettings = new PropertyDefinitionStateAssetSettings {
          Options = new HashSet<PropertyStateAssetOptions>(new[] { PropertyStateAssetOptions.ChangedCallback })
        },
      });

      bar.Properties.Add(new PropertyDefinition {
        Comment = "My Comment",
        Deleted = false,
        Enabled = true,
        Expanded = false,
        Name = "TestArray",
        Replicated = true,
        PropertyType = new PropertyTypeArray {
          ElementCount = 32,
          ElementType = new PropertyTypeStruct {
            StructGuid = baz.Guid
          }
        },
        AssetSettings = new PropertyDefinitionStateAssetSettings(),
      });

      Context context = new Context();
      context.Add(bar);
      context.Add(foo);
      context.Add(baz);

      //Context context = new Context();
      //StructDefinition enchant = new StructDefinition();
      //enchant.AssetPath = "Types/Enchant.asset";
      //enchant.Enabled = true;
      //enchant.Guid = Guid.NewGuid();

      //StructDefinition item = new StructDefinition();
      //item.AssetPath = "Types/Weapon.asset";
      //item.Enabled = true;
      //item.Guid = Guid.NewGuid();
      //item.Properties.Add(new PropertyDefinition {
      //  Name = "Enchants",
      //  Enabled = true,
      //  Replicated = true,
      //  AssetSettings = new PropertyDefinitionStateAssetSettings { },
      //  PropertyType = new PropertyTypeArray {
      //    ElementCount = 2,
      //    ElementType = new PropertyTypeStruct {
      //      StructGuid = enchant.Guid
      //    }
      //  }
      //});

      //context.Add(enchant);
      //context.Add(item);

      context.GenerateCode("Test.cs");
    }
  }
}
