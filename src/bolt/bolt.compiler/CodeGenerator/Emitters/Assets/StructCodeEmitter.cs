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

      CodeTypeDeclaration type;

      type = Generator.DeclareStruct(Decorator.Name);
      type.TypeAttributes = Decorator.BasedOnState ? TypeAttributes.NotPublic : TypeAttributes.Public;
      type.CommentSummary(cm => { cm.CommentDoc(Decorator.Definition.Comment ?? ""); });

      DeclareShimConstructor(type);

      for (int i = 0; i < Decorator.Properties.Count; ++i) {
        PropertyCodeEmitter.Create(Decorator.Properties[i]).EmitStructMembers(type);
      }

      type.DeclareMethod(Decorator.ModifierInterfaceName, "Modify", method => {
        method.Statements.Expr("return new {0}(frame, offsetBytes, offsetObjects)", Decorator.ModifierName);
      });
    }

    public void EmitArray() {
      if (Decorator.BasedOnState) {
        return;
      }

      CodeTypeDeclaration type;

      type = Generator.DeclareStruct(Decorator.ArrayName);
      type.TypeAttributes = TypeAttributes.Public;
      type.DeclareField("Bolt.State.Frame", "frame");
      type.DeclareField("System.Int32", "offsetBytes");
      type.DeclareField("System.Int32", "offsetObjects");
      type.DeclareField("System.Int32", "length");

      type.DeclareConstructor(ctor => {
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

      type.DeclareProperty(typeof(int).FullName, "Length", get => {
        get.Expr("return length");
      });

      type.DeclareProperty(Decorator.Name, "Item", get => {
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

      CodeTypeDeclaration type;

      type = Generator.DeclareClass(Decorator.ModifierName);
      type.TypeAttributes = TypeAttributes.NotPublic;
      type.BaseTypes.Add(Decorator.ModifierInterfaceName);

      DeclareShimConstructor(type);

      // dispose method
      type.DeclareMethod(typeof(void).FullName, "Dispose", method => { }).Attributes = MemberAttributes.Public;

      // other stuff
      for (int i = 0; i < Decorator.Properties.Count; ++i) {
        PropertyCodeEmitter.Create(Decorator.Properties[i]).EmitModifierMembers(type);
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
