using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bolt.Compiler {
  partial class CodeGenerator {
    public CodeTypeDeclaration DeclareInterface(string name, params string[] inherits) {
      return CodeNamespace.DeclareInterface(name, inherits);
    }

    public CodeTypeDeclaration DeclareStruct(string name) {
      return CodeNamespace.DeclareStruct(name);
    }

    public CodeTypeDeclaration DeclareClass(string name) {
      return CodeNamespace.DeclareClass(name);
    }
  }
}
