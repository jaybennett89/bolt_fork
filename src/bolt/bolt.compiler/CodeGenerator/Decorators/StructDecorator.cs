using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class StructDecorator : AssetDecorator<StructDefinition> {
    public int BitCount;
    public StateDecorator SourceState = null;
    public List<PropertyDecorator> Properties = new List<PropertyDecorator>();

    public bool BasedOnState {
      get { return SourceState != null; }
    }

    public string ArrayName {
      get { return Name + "Array"; }
    }
  }
}
