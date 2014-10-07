using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public abstract class AssetCodeEmitter {
    public AssetDecorator Decorator;
    public CodeGenerator Generator { get { return Decorator.Generator; } }
  }
}
