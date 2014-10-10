using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public abstract class PropertyCodeEmitter {
    public PropertyDecorator Decorator;

    public CodeGenerator Generator {
      get { return Decorator.Generator; }
    }

    public virtual string SerializerClassName {
      get { return "PropertySerializer" + Decorator.GetType().Name.Replace("PropertyDecorator", ""); }
    }

    public CodeExpression EmitStatePropertyInitializer(StateDecoratorProperty p) {
      return (@"new Bolt." + SerializerClassName + @"(new Bolt.StatePropertyMetaData {{ ByteOffset = {0}, ByteLength = {1}, ObjectOffset = {2}, Priority = {3}, PropertyPath = ""{4}"", CallbackPaths = {5}, CallbackIndices = {6}, PropertyName = ""{7}"" }})").Expr(
        p.OffsetBytes, // {0}
        Decorator.ByteSize, // {1}
        p.OffsetObjects, // {2}
        Decorator.Definition.Priority, // {3}
        p.CallbackPaths[p.CallbackPaths.Length - 1].Trim('.'), // {4}
        p.CallbackPathsExpression(), // {5}
        p.CreateIndicesExpr(), // {6}
        Decorator.Definition.Name // {7} 
      );
    }

    public virtual string EmitSetPropertyDataArgument() {
      return null;
    }

    public CodeExpression EmitCommandPropertyInitializer() {
      return (@"new Bolt.{0}(new Bolt.CommandPropertyMetaData {{ ByteOffset = {1}, PropertyName = ""{2}"" }})").Expr(
        SerializerClassName,
        Decorator.ByteOffset,
        Decorator.Definition.Name
      );
    }

    public CodeExpression EmitEventPropertyInitializer() {
      return (@"new Bolt.{0}(new Bolt.EventPropertyMetaData {{ ByteOffset = {1}, PropertyName = ""{2}"" }})").Expr(
        SerializerClassName,
        Decorator.ByteOffset,
        Decorator.Definition.Name
      );
    }

    public virtual CodeExpression EmitCustomSerializerInitilization(CodeExpression expression) {
      return null;
    }

    public virtual void EmitStateInterfaceMembers(CodeTypeDeclaration type) {
      EmitSimpleIntefaceMember(type, true, false);
    }

    public virtual void EmitStateMembers(StateDecorator decorator, CodeTypeDeclaration type) {
      EmitForwardStateMember(decorator, type);
    }

    public virtual void EmitStructMembers(CodeTypeDeclaration type) {
      throw new NotImplementedException();
    }

    public virtual void EmitModifierMembers(CodeTypeDeclaration type) {
      throw new NotImplementedException();
    }

    public virtual void EmitModifierInterfaceMembers(CodeTypeDeclaration type) {
      EmitSimpleIntefaceMember(type, true, true);
    }

    public virtual void EmitCommandMembers(CodeTypeDeclaration type, string bytes, string implType) {
      throw new NotImplementedException();
    }

    public virtual void EmitEventMembers(CodeTypeDeclaration type) {
      throw new NotImplementedException();
    }

    public void EmitSimpleIntefaceMember(CodeTypeDeclaration type, bool get, bool set) {
      if (get && set) {
        type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, (_) => { }, (_) => { });
      }
      else if (get) {
        type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, (_) => { }, null);
      }
      else if (set) {
        type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, null, (_) => { });
      }
    }

    protected void EmitForwardStateMember(StateDecorator decorator, CodeTypeDeclaration type) {
      type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, get => {
        get.Expr("return (new {0}(Frames.first, 0, 0)).{1}", decorator.RootStruct.Name, Decorator.Definition.Name);
      });
    }

    protected string CallbackDelegateType {
      get {
        return Decorator.DefiningAsset is StateDecorator
        ? String.Format("Action<{0}>", ((StateDecorator)Decorator.DefiningAsset).InterfaceName)
        : String.Format("Action<{0}>", Decorator.DefiningAsset.Definition.Name);
      }
    }

    public static PropertyCodeEmitter Create(PropertyDecorator decorator) {
      PropertyCodeEmitter emitter;

      emitter = decorator.CreateEmitter();
      emitter.Decorator = decorator;

      return emitter;
    }

  }

  public abstract class PropertyCodeEmitter<T> : PropertyCodeEmitter where T : PropertyDecorator {
    public new T Decorator { get { return (T)base.Decorator; } }
  }


  public abstract class PropertyCodeEmitterSimple<T> : PropertyCodeEmitter<T> where T : PropertyDecorator {
    public abstract string ReadMethod {
      get;
    }

    public abstract string PackMethod {
      get;
    }

    string Get(object data, object offset) {
      return string.Format("return Bolt.Blit.{2}({0}, {1})", data, offset, ReadMethod);
    }

    string Set(object data, object offset) {
      return string.Format("Bolt.Blit.{2}({0}, {1}, value)", data, offset, PackMethod);
    }

    void DeclareProperty(CodeTypeDeclaration type, bool emitSetter) {
      var offset = "offsetBytes + " + Decorator.ByteOffset;
      type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, Get("frame.Data", offset), emitSetter ? Set("frame.Data", offset) : null);
    }

    public override void EmitStructMembers(CodeTypeDeclaration type) {
      DeclareProperty(type, false);
    }

    public override void EmitModifierMembers(CodeTypeDeclaration type) {
      DeclareProperty(type, true);
    }

    public override void EmitCommandMembers(CodeTypeDeclaration type, string bytes, string implType) {
      CodeMemberProperty property;
      property = type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, Get(bytes, Decorator.ByteOffset), Set(bytes, Decorator.ByteOffset));
      property.PrivateImplementationType = new CodeTypeReference(implType);
    }

    public override void EmitEventMembers(CodeTypeDeclaration type) {
      type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, Get("Data", Decorator.ByteOffset), Set("Data", Decorator.ByteOffset));
    }
  }
}
