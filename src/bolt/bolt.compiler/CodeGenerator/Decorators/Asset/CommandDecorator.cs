using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class CommandDecorator : AssetDecorator<CommandDefinition> {
    public override string FactoryInterface {
      get { return "Bolt.ICommandFactory"; }
    }

    public override List<PropertyDecorator> Properties {
      get;
      set;
    }
  }
}
