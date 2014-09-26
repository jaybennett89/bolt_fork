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

    public void EmitStruct() {
      if (Decorator.BasedOnState && Decorator.SourceState.Definition.IsAbstract) {
        return;
      }

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

      str.DeclareMethod(Decorator.ModifierInterfaceName, "Modify", method => {
        method.Statements.Expr("return new {0}(frame, offsetBytes, offsetObjects)", Decorator.ModifierName);
      });
    }

    public void EmitArray() {
      if (Decorator.BasedOnState) {
        return;
      }

      CodeTypeDeclaration arr;

      arr = Generator.DeclareStruct(Decorator.ArrayName);
      arr.TypeAttributes = TypeAttributes.Public;
      arr.DeclareField("Bolt.State.Frame", "frame");
      arr.DeclareField("System.Int32", "offsetBytes");
      arr.DeclareField("System.Int32", "offsetObjects");
      arr.DeclareField("System.Int32", "length");

      arr.DeclareConstructor(ctor => {
        ctor.Attributes = MemberAttributes.Assembly;

        ctor.DeclareParameter("Bolt.State.Frame", "frame");
        ctor.DeclareParameter("System.Int32", "offsetBytes");
        ctor.DeclareParameter("System.Int32", "offsetObjects");
        ctor.DeclareParameter("System.Int32", "length");

        ctor.Statements.Assign("this.frame".Expr(), "frame".Expr());
        ctor.Statements.Assign("this.offsetBytes".Expr(), "offsetBytes".Expr());
        ctor.Statements.Assign("this.offsetObjects".Expr(), "offsetObjects".Expr());
        ctor.Statements.Assign("this.length".Expr(), "length".Expr());
      });

      arr.DeclareProperty(typeof(int).FullName, "Length", get => {
        get.Expr("return length");
      });

      arr.DeclareProperty(Decorator.Name, "Item", get => {
        get.Expr("if (index < 0 || index >= length) throw new IndexOutOfRangeException()");
        get.Expr("return new {0}(frame, offsetBytes + (index * {1}), offsetObjects + (index * {2}))", Decorator.Name, Decorator.ByteSize, Decorator.ObjectSize);
      }).DeclareParameter(typeof(int).FullName, "index");
    }

    public void EmitModifierInterface() {
      CodeTypeDeclaration imod;

      imod = Generator.DeclareInterface(Decorator.ModifierInterfaceName);
      imod.TypeAttributes = TypeAttributes.Public | TypeAttributes.Interface;

      if (Decorator.BasedOnState && Decorator.SourceState.HasParent) {
        imod.BaseTypes.Add(Generator.FindStruct(Decorator.SourceState.Parent.Guid).ModifierInterfaceName);
      }
      else {
        imod.BaseTypes.Add("Bolt.IStateModifier");
      }

      for (int i = 0; i < Decorator.Properties.Count; ++i) {
        if (Decorator.BasedOnState) {
          // if this was not defined on this asset skip it (as it will be inherited from the parent)
          if (Decorator.Properties[i].DefiningAsset.Guid != Decorator.SourceState.Guid) {
            continue;
          }
        }

        PropertyCodeEmitter.Create(Decorator.Properties[i]).EmitModifierInterfaceMembers(imod);
      }
    }

    public void EmitModifier() {
      // check if this is based on an abstract state and if true then dont emit anything
      if (Decorator.BasedOnState && Decorator.SourceState.Definition.IsAbstract) {
        return;
      }

      CodeTypeDeclaration mod;

      mod = Generator.DeclareClass(Decorator.ModifierName);
      mod.TypeAttributes = TypeAttributes.NotPublic;
      mod.BaseTypes.Add(Decorator.ModifierInterfaceName);

      DeclareShimConstructor(mod);

      for (int i = 0; i < Decorator.Properties.Count; ++i) {
        PropertyCodeEmitter.Create(Decorator.Properties[i]).EmitModifierMembers(mod);
      }
    }

    void DeclareShimConstructor(CodeTypeDeclaration type) {
      type.DeclareField("Bolt.State.Frame", "frame");
      type.DeclareField("System.Int32", "offsetBytes");
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
