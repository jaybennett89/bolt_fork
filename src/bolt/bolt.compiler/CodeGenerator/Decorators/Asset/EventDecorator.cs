using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class EventDecorator : AssetDecorator<EventDefinition> {
    public override string FactoryInterface {
      get { return "Bolt.IEventFactory"; }
    }

    public override List<PropertyDecorator> Properties {
      get;
      set;
    }
  }
}
