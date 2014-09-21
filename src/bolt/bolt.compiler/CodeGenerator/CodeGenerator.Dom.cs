using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bolt.Compiler {
  partial class CodeGenerator {
    public CodeTypeDeclaration DeclareInterface(string name, params string[] inherits) {
      CodeTypeDeclaration td;

      td = new CodeTypeDeclaration(name);
      td.IsInterface = true;
      td.BaseTypes.AddRange(inherits.Select(x => new CodeTypeReference(x)).ToArray());
      td.TypeAttributes = TypeAttributes.Public | TypeAttributes.Interface;

      CodeNamespace.Types.Add(td);

      return td;
    }

    public CodeTypeDeclaration DeclareStruct(string name) {
      CodeTypeDeclaration td;

      td = new CodeTypeDeclaration(name);
      td.IsStruct = true;
      td.TypeAttributes = TypeAttributes.Public;

      CodeNamespace.Types.Add(td);

      return td;
    }

    public CodeTypeDeclaration DeclareClass(string name) {
      CodeTypeDeclaration td;

      td = new CodeTypeDeclaration(name);
      td.IsClass = true;
      td.TypeAttributes = TypeAttributes.Public;

      CodeNamespace.Types.Add(td);

      return td;
    }
  }
}
