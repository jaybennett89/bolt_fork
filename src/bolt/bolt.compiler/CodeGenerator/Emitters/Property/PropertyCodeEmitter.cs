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
      get { return "Bolt.PropertySerializer" + Decorator.GetType().Name.Replace("PropertyDecorator", ""); }
    }

    public CodeExpression GetCreateSerializerExpression() {
      return "new {0}()".Expr(SerializerClassName);
    }

    public virtual void GetAddSettingsArgument(List<string> settings) {

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

    public void EmitAddSettings(CodeExpression expr, CodeStatementCollection statements, StateProperty sp) {
      List<string> settings = new List<string>();

      // property settings 
      if ((Decorator.DefiningAsset is StateDecorator) || (Decorator.DefiningAsset is StructDecorator)) {
        settings.Add(string.Format("new Bolt.PropertySettings({0}, \"{1}\", Bolt.PropertyModes.State)", sp.OffsetBytes, Decorator.Definition.Name));
        settings.Add(string.Format("new Bolt.PropertyStateSettings({0}, {1}, {2}, \"{3}\", {4}, {5})",
          Decorator.Definition.Priority,
          Decorator.ByteSize,
          sp.OffsetObjects,
          sp.PropertyPath,
          sp.CallbackPathsExpression(),
          sp.CallbackIndicesExpression()
        ));
      }
      else {
        settings.Add(string.Format(
          "new Bolt.PropertySettings({0}, \"{1}\", Bolt.PropertyModes.{2})", 
          Decorator.ByteOffset, 
          Decorator.Definition.Name,
          Decorator.DefiningAsset is EventDecorator ? "Event" : "Command"
        ));
      }

      // mecanim for states settings
      if ((Decorator.DefiningAsset is StateDecorator) && Decorator.Definition.PropertyType.MecanimApplicable) {
        var s = Decorator.Definition.StateAssetSettings;

        settings.Add(string.Format("new Bolt.PropertyMecanimSettings(Bolt.MecanimMode.{0}, Bolt.MecanimDirection.{1}, {2}f, {3})",
          s.MecanimMode,
          s.MecanimDirection,
          s.MecanimDamping,
          s.MecanimLayer
        ));
      }

      // collecting property specific settings
      GetAddSettingsArgument(settings);

      for (int n = 0; n < settings.Count; ++n) {
        statements.Add(new CodeMethodInvokeExpression(new CodeCastExpression(SerializerClassName, expr), "AddSettings", new CodeSnippetExpression(settings[n])));
      }
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
