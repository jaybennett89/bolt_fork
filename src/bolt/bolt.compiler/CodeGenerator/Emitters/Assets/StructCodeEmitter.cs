using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bolt.Compiler {
  public class StructCodeEmitter : AssetCodeEmitter {
    public new StructDecorator Decorator {
      get { return (StructDecorator)base.Decorator; }
      set { base.Decorator = value; }
    }

    public void EmitPropertySetup(CodeTypeDeclaration type) {
      type.DeclareMethod(typeof(void).FullName, "PropertySetup", method => {
        method.Attributes = MemberAttributes.Static | MemberAttributes.Assembly;
        method.DeclareParameter("Bolt.SerializerGroup", "grp");
        method.DeclareParameter("Stack<string>", "path");

        DomBlock block = new DomBlock(method.Statements);

        for (int i = 0; i < Decorator.Properties.Count; ++i) {
          block.Stmts.Comment("");
          block.Stmts.Comment(Decorator.Properties[i].Definition.Name);
          PropertyCodeEmitter.Create(Decorator.Properties[i]).EmitPropertySetup(block, "grp", "path");
        }
      });
    }

    public void EmitStruct() {
      if (Decorator.BasedOnState && Decorator.SourceState.Definition.IsAbstract) {
        return;
      }

      CodeTypeDeclaration type;

      type = Generator.DeclareClass(Decorator.Name);
      type.BaseTypes.Add("Bolt.NetworkObject");
      type.TypeAttributes = Decorator.BasedOnState ? TypeAttributes.NotPublic : TypeAttributes.Public;

      for (int i = 0; i < Decorator.Properties.Count; ++i) {
        PropertyCodeEmitter.Create(Decorator.Properties[i]).EmitObjectMembers(type);
      }

      type.DeclareMethod(Decorator.Name, "Modify", method => {
        method.DeclareModifyObsolete();
        method.Statements.Expr("return this");
      });

      EmitPropertySetup(type);
    }

    void DeclareShimConstructor(CodeTypeDeclaration type) {
      type.DeclareField("Bolt.State", "state");
      type.DeclareField("System.Int32", "offsetStorage");
      type.DeclareField("System.Int32", "offsetObjects");

      type.DeclareConstructor(ctor => {
        ctor.Attributes = MemberAttributes.Assembly;

        ctor.DeclareParameter("Bolt.State.Frame", "frame");
        ctor.DeclareParameter("System.Int32", "offsetBytes");
        ctor.DeclareParameter("System.Int32", "offsetObjects");

        ctor.Statements.Assign("this.frame".Expr(), "frame".Expr());
        ctor.Statements.Assign("this.offsetBytes".Expr(), "offsetBytes".Expr());
        ctor.Statements.Assign("this.offsetObjects".Expr(), "offsetObjects".Expr());
      });
    }

  }
}
