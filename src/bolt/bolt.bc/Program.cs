using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bolt.Compiler;

namespace Bolt.Bc {
  class Program {
    static void Main(string[] args) {
      PropertyFilterDefinition filter = new PropertyFilterDefinition {
        Index = 0,
        Name = "Default",
        Enabled = true
      };

      StructDefinition enchant = new StructDefinition();
      enchant.Name = "Types/Enchant.asset";
      enchant.Enabled = true;
      enchant.Guid = Guid.NewGuid();
      enchant.Properties.Add(new PropertyDefinition {
        Name = "Value",
        Enabled = true,
        Replicated = true,
        AssetSettings = new PropertyDefinitionStateAssetSettings { },
        PropertyType = new PropertyTypeFloat { }
      });

      enchant.Properties.Add(new PropertyDefinition {
        Name = "Type",
        Enabled = true,
        Replicated = true,
        AssetSettings = new PropertyDefinitionStateAssetSettings { },
        PropertyType = new PropertyTypeString { MaxLength = 16 }
      });

      StructDefinition item = new StructDefinition();
      item.Name = "Types/Item.asset";
      item.Enabled = true;
      item.Guid = Guid.NewGuid();
      item.Properties.Add(new PropertyDefinition {
        Name = "Enchant",
        Enabled = true,
        Replicated = true,
        AssetSettings = new PropertyDefinitionStateAssetSettings { },
        PropertyType = new PropertyTypeStruct { StructGuid = enchant.Guid }
      });

      StructDefinition buff = new StructDefinition();
      buff.Name = "Types/Buff.asset";
      buff.Enabled = true;
      buff.Guid = Guid.NewGuid();

      buff.Properties.Add(new PropertyDefinition {
        Name = "Value",
        Enabled = true,
        Replicated = true,
        AssetSettings = new PropertyDefinitionStateAssetSettings { },
        PropertyType = new PropertyTypeFloat { }
      });

      buff.Properties.Add(new PropertyDefinition {
        Name = "Type",
        Enabled = true,
        Replicated = true,
        AssetSettings = new PropertyDefinitionStateAssetSettings { Filters = 1 },
        PropertyType = new PropertyTypeString { MaxLength = 16 }
      });

      buff.Properties.Add(new PropertyDefinition {
        Name = "Timer",
        Enabled = true,
        Replicated = true,
        AssetSettings = new PropertyDefinitionStateAssetSettings { Filters = 1 },
        PropertyType = new PropertyTypeFloat {  }
      });

      StateDefinition character = new StateDefinition();
      character.Name = "Types/Character.asset";
      character.Enabled = true;
      character.Guid = Guid.NewGuid();

      character.Properties.Add(new PropertyDefinition {
        Name = "Buffs",
        Enabled = true,
        Replicated = true,
        AssetSettings = new PropertyDefinitionStateAssetSettings { },
        PropertyType = new PropertyTypeArray {
          ElementCount = 16,
          ElementType = new PropertyTypeStruct {
            StructGuid = buff.Guid
          }
        }
      });

      character.Properties.Add(new PropertyDefinition {
        Name = "Debuffs",
        Enabled = true,
        Replicated = true,
        AssetSettings = new PropertyDefinitionStateAssetSettings { },
        PropertyType = new PropertyTypeArray {
          ElementCount = 16,
          ElementType = new PropertyTypeStruct {
            StructGuid = buff.Guid
          }
        }
      });

      character.Properties.Add(new PropertyDefinition {
        Name = "Inventory",
        Enabled = true,
        Replicated = true,
        AssetSettings = new PropertyDefinitionStateAssetSettings { },
        PropertyType = new PropertyTypeArray {
          ElementCount = 256,
          ElementType = new PropertyTypeStruct {
            StructGuid = item.Guid
          }
        }
      });

    }
  }
}
