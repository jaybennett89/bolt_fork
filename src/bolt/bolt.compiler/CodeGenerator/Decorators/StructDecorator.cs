using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class StructDecorator : AssetDecorator<StructDefinition> {
    public int BitCount;
    public List<PropertyDecorator> Properties = new List<PropertyDecorator>();
  }
}
