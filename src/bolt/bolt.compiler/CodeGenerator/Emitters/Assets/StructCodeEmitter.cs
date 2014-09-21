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
      EmitModifier();
    }

    void EmitStruct() {
      CodeTypeDeclaration str;

      str = Generator.DeclareStruct(Decorator.Name);
      str.TypeAttributes = Decorator.BasedOnState ? TypeAttributes.NotPublic : TypeAttributes.Public;
      str.CommentSummary(m => {
        m.CommentDoc(Decorator.Definition.Comment);
        m.CommentDoc("(Properties={0} ByteSize={1})", Decorator.Properties.Count, Decorator.ByteSize);
      });

      DeclareShimConstructor(str);

      for (int i = 0; i < Decorator.Properties.Count; ++i) {
        PropertyCodeEmitter.Create(Decorator.Properties[i]).EmitShimMembers(str);
      }

      str.DeclareMethod(Decorator.ModifierName, "Modify", method => {
        method.Statements.Expr("return new {0}(data, offset)", Decorator.ModifierName);
      });
    }

    void EmitArray() {
      if (Decorator.BasedOnState) {
        return;
      }

      CodeTypeDeclaration arr;

      arr = Generator.DeclareStruct(Decorator.ArrayName);
      arr.TypeAttributes = TypeAttributes.Public;
      arr.DeclareField("System.Byte[]", "data");
      arr.DeclareField("System.Int32", "offset");
      arr.DeclareField("System.Int32", "length");

      arr.DeclareConstructor(ctor => {
        ctor.Attributes = MemberAttributes.Assembly;

        ctor.DeclareParameter("System.Byte[]", "data");
        ctor.DeclareParameter("System.Int32", "offset");
        ctor.DeclareParameter("System.Int32", "length");

        ctor.Statements.Assign("this.data".Expr(), "data".Expr());
        ctor.Statements.Assign("this.offset".Expr(), "offset".Expr());
        ctor.Statements.Assign("this.length".Expr(), "length".Expr());
      });

      arr.DeclareProperty(typeof(int).FullName, "Length", get => {
        get.Expr("return length");
      });

      arr.DeclareProperty(Decorator.Name, "Item", get => {
        get.Expr("if (index < 0 || index >= length) throw new IndexOutOfRangeException()");
        get.Expr("return new {0}(data, offset + (index * {1}))", Decorator.Name, Decorator.ByteSize);
      }).DeclareParameter(typeof(int).FullName, "index");
    }

    void EmitModifier() {
      CodeTypeDeclaration mod;

      mod = Generator.DeclareClass(Decorator.ModifierName);
      mod.TypeAttributes = TypeAttributes.Public;
      mod.BaseTypes.Add("System.IDisposable");

      DeclareShimConstructor(mod);

      for (int i = 0; i < Decorator.Properties.Count; ++i) {
        PropertyCodeEmitter.Create(Decorator.Properties[i]).EmitModifierMembers(mod);
      }
    }

    void DeclareShimConstructor(CodeTypeDeclaration type) {
      type.DeclareField("System.Byte[]", "data");
      type.DeclareField("System.Int32", "offset");

      type.DeclareConstructor(ctor => {
        ctor.Attributes = MemberAttributes.Assembly;

        ctor.DeclareParameter("System.Byte[]", "data");
        ctor.DeclareParameter("System.Int32", "offset");

        ctor.Statements.Assign("this.data".Expr(), "data".Expr());
        ctor.Statements.Assign("this.offset".Expr(), "offset".Expr());
      });
    }
  }
}
