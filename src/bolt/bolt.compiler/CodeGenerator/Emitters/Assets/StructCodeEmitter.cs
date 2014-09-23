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

    public override void EmitTypes() {
      EmitStruct();
      EmitArray();
      EmitModifier();
    }

    void EmitByteMasks(CodeStatementCollection stmts) {
      for (int i = 0; i < Decorator.Properties.Count; ++i) {
        PropertyDecorator p = Decorator.Properties[i];

      }

      stmts.Expr("structIndex += 1");

      for (int i = 0; i < Decorator.Properties.Count; ++i) {
        PropertyDecorator p = Decorator.Properties[i];
        PropertyDecoratorArray arrayProperty = p as PropertyDecoratorArray;
        PropertyDecoratorStruct structProperty = p as PropertyDecoratorStruct;

        if (structProperty != null) {
          stmts.Comment(p.Definition.Name);
          stmts.Expr("{0}.InitByteMask(masks, ref structIndex, ref byteOffset)", structProperty.Struct.Name);
        }
        else
          if (arrayProperty != null) {
            PropertyTypeStruct elementType = arrayProperty.PropertyType.ElementType as PropertyTypeStruct;

            if (elementType != null) {
              StructDecorator elementStruct = Generator.FindStruct(elementType.StructGuid);
              stmts.Comment(p.Definition.Name);
              stmts.Expr("for(int i = 0; i < {0}; ++i) {1}.InitByteMask(masks, ref structIndex, ref byteOffset)", arrayProperty.PropertyType.ElementCount, elementStruct.Name);
            }
          }
      }
    }

    void EmitStruct() {
      CodeTypeDeclaration str;

      str = Generator.DeclareStruct(Decorator.Name);
      str.TypeAttributes = Decorator.BasedOnState ? TypeAttributes.NotPublic : TypeAttributes.Public;
      str.CommentSummary(m => {
        m.CommentDoc(Decorator.Definition.Comment);
        m.CommentDoc("(Properties={0} ByteSize={1})", Decorator.Properties.Count, Decorator.FrameSize);
      });

      DeclareShimConstructor(str);

      for (int i = 0; i < Decorator.Properties.Count; ++i) {
        PropertyCodeEmitter.Create(Decorator.Properties[i]).EmitShimMembers(str);
      }

      str.DeclareMethod(Decorator.ModifierName, "Modify", method => {
        method.Statements.Expr("return new {0}(data, offset)", Decorator.ModifierName);
      });

      str.DeclareMethod(typeof(void).FullName, "InitByteMask", method => {
        method.DeclareParameter("Bolt.State.ByteMasks[]", "masks");
        method.DeclareParameter("ref int", "structIndex");
        method.DeclareParameter("ref int", "byteOffset");

        EmitByteMasks(method.Statements);

      }).Attributes = MemberAttributes.Static | MemberAttributes.Assembly;
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
        get.Expr("return new {0}(data, offset + (index * {1}))", Decorator.Name, Decorator.FrameSize);
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
