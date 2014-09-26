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

    public virtual CodeExpression CreatePropertyArrayInitializerExpression(int byteOffset, int objectOffset) {
      throw new NotImplementedException();
    }

    public virtual void EmitStateInterfaceMembers(CodeTypeDeclaration type) {
      EmitSimpleIntefaceMember(type, true, false);
      EmitChangedCallbackProperty(type, true);
    }

    public virtual void EmitStateClassMembers(StateDecorator decorator, CodeTypeDeclaration type) {
      EmitForwardStateMember(decorator, type);

      var stateSettings = Decorator.Definition.StateAssetSettings;
      if (stateSettings.Callback) {
        type.DeclareProperty(CallbackDelegateType, Decorator.Definition.ChangedCallbackName, (get) => {
          get.Expr("return (new {0}(Frames.first, 0, 0)).{1}", decorator.RootStruct.Name, Decorator.Definition.ChangedCallbackName);
        }, (set) => {
          set.Expr("(new {0}(Frames.first, 0, 0)).{1} = value", decorator.RootStruct.Name, Decorator.Definition.ChangedCallbackName);
        });
      }
    }

    public virtual void EmitShimMembers(CodeTypeDeclaration type) {
      throw new NotImplementedException();
    }

    public virtual void EmitModifierMembers(CodeTypeDeclaration type) {
      throw new NotImplementedException();
    }

    public virtual void EmitModifierInterfaceMembers(CodeTypeDeclaration type) {
      EmitSimpleIntefaceMember(type, true, false);
    }

    protected void EmitSimpleIntefaceMember(CodeTypeDeclaration type, bool get, bool set) {
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

    protected void EmitChangedCallbackProperty(CodeTypeDeclaration type, bool code) {
      var stateSettings = Decorator.Definition.StateAssetSettings;
      if (stateSettings.Callback) {
        type.DeclareProperty(CallbackDelegateType, Decorator.Definition.ChangedCallbackName, (get) => {
          if (code) {
            get.Expr("return ({0}) frame.Objects[offsetObjects + {1}]", CallbackDelegateType, Decorator.ObjectOffset);
          }
        }, (set) => {
          if (code) {
            set.Expr("frame.Objects[offsetObjects + {0}] = value;", Decorator.ObjectOffset);
          }
        });
      }
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
