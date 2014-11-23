using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class EventDecorator : AssetDecorator<EventDefinition> {
    public override string FactoryInterface {
      get { return "Bolt.IEventFactory"; }
    }

    public override string BaseClass {
      get { return "Bolt.NetworkEvent"; }
    }

    public override bool EmitPropertyChanged {
      get { return false; }
    }

    public override List<PropertyDecorator> Properties {
      get;
      set;
    }

    public string ListenerInterface {
      get { return "I" + Name + "Listener"; }
    }

    public EventDecorator(EventDefinition def) {
      Definition = def;
    }
  }
}
