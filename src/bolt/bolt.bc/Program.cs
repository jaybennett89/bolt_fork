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
        Name = "Test1",
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
        Name = "Test2",
        Replicated = true,
        PropertyType = new PropertyTypeString(),
        AssetSettings = new PropertyDefinitionStateAssetSettings(),
      });

      Context context = new Context();
      context.Add(bar);
      context.Add(foo);
      context.Add(baz);

      context.GenerateCode("Test.cs");
    }
  }
}
