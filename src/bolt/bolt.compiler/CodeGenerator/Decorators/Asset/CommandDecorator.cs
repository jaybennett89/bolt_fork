using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class CommandDecorator : AssetDecorator<CommandDefinition> {
    public int InputByteSize;
    public int ResultByteSize;

    public List<PropertyDecorator> InputProperties = new List<PropertyDecorator>();
    public List<PropertyDecorator> ResultProperties = new List<PropertyDecorator>();

    public string FactoryName {
      get { return Definition.Name + "Factory"; }
    }

    public string InterfaceName {
      get { return "I" + Definition.Name; }
    }

    public string InputInterfaceName {
      get { return InterfaceName + "Input"; }
    }

    public string ResultInterfaceName {
      get { return InterfaceName + "Result"; }
    }
  }
}
