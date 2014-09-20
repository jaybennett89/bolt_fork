using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bolt.Compiler {
  public class StructCodeEmitter : AssetCodeEmitter {
    public StructDecorator Decorator;

    public void Emit() {
      EmitStruct();
      EmitArray();
    }

    void EmitStruct() {
      CodeTypeDeclaration td;

      td = Generator.DeclareStruct(Decorator.Name);
      td.TypeAttributes = Decorator.BasedOnState ? TypeAttributes.NotPublic : TypeAttributes.Public;
      td.DeclareField("System.Byte[]", "data");
      td.DeclareField("System.Int32", "offset");

      td.DeclareConstructor(ctor => {
        ctor.Attributes = MemberAttributes.Assembly;

        ctor.DeclareParameter("System.Byte[]", "data");
        ctor.DeclareParameter("System.Int32", "offset");

        ctor.Statements.Assign("this.data".Expr(), "data".Expr());
        ctor.Statements.Assign("this.offset".Expr(), "offset".Expr());
      });
    }

    void EmitArray() {
      if (Decorator.BasedOnState) {
        return;
      }

      CodeTypeDeclaration td;

      td = Generator.DeclareStruct(Decorator.ArrayName);
      td.TypeAttributes = TypeAttributes.Public;
      td.DeclareField("System.Byte[]", "data");
      td.DeclareField("System.Int32", "offset");
      td.DeclareField("System.Int32", "length");

      td.DeclareConstructor(ctor => {
        ctor.Attributes = MemberAttributes.Assembly;

        ctor.DeclareParameter("System.Byte[]", "data");
        ctor.DeclareParameter("System.Int32", "offset");
        ctor.DeclareParameter("System.Int32", "length");

        ctor.Statements.Assign("this.data".Expr(), "data".Expr());
        ctor.Statements.Assign("this.offset".Expr(), "offset".Expr());
        ctor.Statements.Assign("this.length".Expr(), "length".Expr());
      });
    }
  }
}
