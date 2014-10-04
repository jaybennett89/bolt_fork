using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class EventDecorator : AssetDecorator<EventDefinition> {
    public List<PropertyDecorator> Properties = new List<PropertyDecorator>();

    public int ByteSize {
      get { return Properties.Select(x => x.ByteSize).Sum(); }
    }
  }
}
