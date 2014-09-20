using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public abstract class PropertyCodeEmitter {
    public CodeGenerator Generator;
    public PropertyDecorator Decorator;

    public virtual void EmitInterfaceProperty(CodeTypeDeclaration iface) {
      iface.DeclareProperty(Decorator.PropertyTypeReference, Decorator.Definition.Name, (stms) => { }, (stms) => { });
    }
  }
}
