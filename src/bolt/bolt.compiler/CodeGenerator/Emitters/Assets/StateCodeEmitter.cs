using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;

namespace Bolt.Compiler {
  public class StateCodeEmitter : AssetCodeEmitter {
    public StateDecorator Decorator;

    public void Emit() {
      EmitInterface();
    }

    void EmitInterface() {
      // create interface 
      CodeTypeDeclaration iface = Generator.DeclareInterface(Decorator.InterfaceName, CalulateInterfaceBaseTypes());

      foreach (PropertyDecorator property in Decorator.Properties) {
        if (property.DefiningAsset.Guid == Decorator.Guid) {
          PropertyCodeEmitter emitter;
          emitter = property.Definition.PropertyType.CreateCodeEmitter();
          emitter.Decorator = property;
          emitter.Generator = Generator;
          emitter.EmitInterfaceProperty(iface);
        }
      }
    }

    string[] CalulateInterfaceBaseTypes() {
      if (Decorator.HasParent) {
        return new string[] { Decorator.Parent.InterfaceName };
      }
      else {
        return new string[] { "IState" };
      }
    }
  }
}
