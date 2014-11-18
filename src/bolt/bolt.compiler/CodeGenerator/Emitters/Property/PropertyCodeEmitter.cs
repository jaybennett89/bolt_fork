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

    public virtual string StorageField {
      get { return GetType().Name.Replace("PropertyCodeEmitter", ""); }
    }

    public virtual string SerializerClassName {
      get { return "Bolt.PropertySerializer" + Decorator.GetType().Name.Replace("PropertyDecorator", ""); }
    }

    public virtual void AddSettingsArgument(List<string> settings) {

    }

    public virtual void EmitStateInterfaceMembers(CodeTypeDeclaration type) {
      EmitSimpleIntefaceMember(type, true, true);
    }

    public virtual void EmitStateMembers(StateDecorator decorator, CodeTypeDeclaration type) {
      EmitForwardStateMember(decorator, type, true);
    }

    public virtual void EmitObjectMembers(CodeTypeDeclaration type) {
      type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, get => {
        get.Expr("return CurrentFrame.Storage[this.OffsetStorage + {0}].{1}", Decorator.OffsetStorage, StorageField);
      }, set => {
        set.Expr("CurrentFrame.Storage[this.OffsetStorage + {0}].{1} = value", Decorator.OffsetStorage, StorageField);

        if (Decorator.DefiningAsset.IsStateOrStruct) {
          set.Expr("CurrentFrame.PropertyChanged(this.OffsetSerializers + {0})", Decorator.OffsetSerializers);
        }
      });
    }

    public virtual void EmitCommandMembers(CodeTypeDeclaration type, string bytes, string implType) {
    }

    public virtual void EmitEventMembers(CodeTypeDeclaration type) {
    }

    public virtual void EmitPropertySetup(DomBlock block, string group, string path) {
      string tmp = block.TempVar();
      block.Stmts.Expr("{0} {1}", SerializerClassName, tmp);
      block.Stmts.Expr("{1} = new {0}()", SerializerClassName, tmp);

      EmitAddSettings(tmp.Expr(), block.Stmts);

      block.Stmts.Expr("{0}.AddSerializer({1}, {2}, {3}, {4}, {5})", group, tmp, this.Decorator.RequiredStorage, this.Decorator.RequiredObjects, this.Decorator.RequiredSerializers, path);
    }

    public void EmitAddSettings(CodeExpression expr, CodeStatementCollection statements) {
      List<string> settings = new List<string>();

      // serializer settings
      settings.Add(string.Format(
        "new Bolt.PropertySerializerSettings(\"{0}\", {1}, Bolt.PropertyModes.{2})",
        Decorator.Definition.Name,
        Decorator.Definition.Priority,
        Decorator.DefiningAsset.PropertyMode
      ));

      // command settings
      if (Decorator.DefiningAsset is CommandDecorator) {
        settings.Add(Generator.CreateCommandSettings(Decorator.Definition));
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
      AddSettingsArgument(settings);

      // emit add settings calls
      for (int n = 0; n < settings.Count; ++n) {
        statements.Add(new CodeMethodInvokeExpression(expr, "AddSettings", new CodeSnippetExpression(settings[n])));
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

    protected void EmitForwardStateMember(StateDecorator decorator, CodeTypeDeclaration type, bool allowSetter) {
      Action<CodeStatementCollection> setter = null;

      if (allowSetter) {
        setter = set => {
          set.Expr("_Root.{0} = value", Decorator.Definition.Name);
        };
      }

      type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, get => {
        get.Expr("return _Root.{0}", Decorator.Definition.Name);
      }, setter);
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
}
