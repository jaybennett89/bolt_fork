using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class EventDecorator : AssetDecorator<EventDefinition> {
    public int ByteSize;
    public List<PropertyDecorator> Properties = new List<PropertyDecorator>();

    public string FactoryName {
      get { return Definition.Name + "Factory"; }
    }

    public string ListenerName {
      get { return "I" + Definition.Name + "Listener"; }
    }
  }
}
