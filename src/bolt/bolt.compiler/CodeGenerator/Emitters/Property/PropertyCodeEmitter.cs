﻿using System;
using System.CodeDom;
using System.Collections.Generic;

namespace Bolt.Compiler {
  public abstract class PropertyCodeEmitter {
    public struct Offsets {
      public CodeExpression OffsetStorage;
      public CodeExpression OffsetObjects;
      public CodeExpression OffsetProperties;
    }

    public PropertyDecorator Decorator;

    public CodeGenerator Generator {
      get { return Decorator.Generator; }
    }

    public virtual string StorageField {
      get { return GetType().Name.Replace("PropertyCodeEmitter", ""); }
    }

    public virtual string SerializerClassName {
      get { return Decorator.PropertyClassName; }
    }

    public virtual void AddSettings(CodeExpression expr, CodeStatementCollection statements) {

    }

    public virtual void EmitStateInterfaceMembers(CodeTypeDeclaration type) {
      EmitSimpleIntefaceMember(type, true, true);
    }

    public virtual void EmitStateMembers(StateDecorator decorator, CodeTypeDeclaration type) {
      EmitForwardStateMember(decorator, type, true);
    }

    public virtual void EmitObjectMembers(CodeTypeDeclaration type) {
      EmitSimplePropertyMembers(type, new CodeSnippetExpression("Storage"), null, true);
    }

    public virtual void EmitCommandMembers(CodeTypeDeclaration type, CodeSnippetExpression storage, CodeTypeReference interfaceType, string suffix) {
    }

    public virtual void EmitEventMembers(CodeTypeDeclaration type) {
    }

    protected void EmitVerifySerializer(CodeStatementCollection stmts, string suffix) {
      //stmts.IfDef("DEBUG", block => {
      //  block.Expr(
      //    "this.VerifySerializer{5}(typeof({0}), \"{1}\", this.OffsetSerializers + {2}, this.OffsetStorage + {3}, this.OffsetObjects + {4})",
      //    this.SerializerClassName,
      //    this.Decorator.Definition.Name,
      //    this.Decorator.OffsetSerializers,
      //    this.Decorator.OffsetStorage,
      //    this.Decorator.OffsetObjects,
      //    suffix
      //  );
      //});
    }

    public void EmitSimplePropertyMembers(CodeTypeDeclaration type, CodeSnippetExpression storage, CodeTypeReference interfaceType, bool changed) {
      var index = new CodeIndexerExpression(storage.Field("Values"), "this.OffsetStorage + {0}".Expr(Decorator.OffsetStorage));

      type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, get => {
        get.Add(
          new CodeMethodReturnStatement(
            new CodeFieldReferenceExpression(index, StorageField)
          )
        );
      }, set => {
        set.Add(new CodeAssignStatement(
          new CodeFieldReferenceExpression(index, StorageField),
          new CodeVariableReferenceExpression("value")
        ));

        if (changed) {
          EmitPropertyChanged(set, storage);
        }

      }).PrivateImplementationType = interfaceType;
    }

    public void EmitPropertyChanged(CodeStatementCollection stmt, CodeExpression storage) {
      if (Decorator.DefiningAsset.EmitPropertyChanged) {
        stmt.Add(storage.Call("PropertyChanged", "this.OffsetProperties + {0}".Expr(Decorator.OffsetProperties)));
      }
    }

    public virtual void EmitMetaSetup(DomBlock block) {
      Offsets offsets = new Offsets();
      offsets.OffsetStorage = "{0} /*storage:{1}*/".Expr(Decorator.OffsetStorage, Decorator.RequiredStorage);
      offsets.OffsetProperties = "{0} /*properties:{1}*/".Expr(Decorator.OffsetProperties, Decorator.RequiredProperties);
      offsets.OffsetObjects = "{0} /*objects:{1}*/".Expr(Decorator.OffsetObjects, Decorator.RequiredObjects);

      EmitMetaSetup(block, offsets);
    }

    public virtual void EmitMetaSetup(DomBlock block, Offsets offsets) {
      var tmp = block.Stmts.Var(SerializerClassName, block.TempVar());

      block.Stmts.Assign(tmp, SerializerClassName.New());
      block.Stmts.Assign(tmp.Field("PropertyMeta"), "this".Expr());

      EmitAddSettings(tmp, block.Stmts, offsets);

      int filters = (1 << 30);

      if (Decorator.Definition.Controller) {
        filters |= (1 << 31);
      }

      block.Stmts.Add("this".Expr().Call("AddProperty", offsets.OffsetProperties, offsets.OffsetObjects, tmp));
    }

    public virtual void EmitObjectSetup(DomBlock block) {
      Offsets offsets = new Offsets();
      offsets.OffsetStorage = "offsets.OffsetStorage + {0} /*storage:{1}*/".Expr(Decorator.OffsetStorage, Decorator.RequiredStorage);
      offsets.OffsetObjects = "offsets.OffsetObjects + {0} /*object:{1}*/".Expr(Decorator.OffsetObjects, Decorator.RequiredObjects);
      offsets.OffsetProperties = "offsets.OffsetProperties + {0} /*properties:{1}*/".Expr(Decorator.OffsetProperties, Decorator.RequiredProperties);

      EmitObjectSetup(block, offsets);
    }

    public virtual void EmitObjectSetup(DomBlock block, Offsets offsets) {
      Assert.True(this.Decorator.RequiredObjects == 0);
      block.Stmts.Comment("EMPTY");

      // add storage
      //block.Stmts.Expr("{0}.AddSerializer({1})", group, this.Decorator.RequiredStorage);
    }

    public void EmitInterpolationSettings(CodeExpression expr, CodeStatementCollection statements) {
      var s = Decorator.Definition.StateAssetSettings;
      var c = Decorator.Definition.CommandAssetSettings;

      if (s != null) {
        if (s.SmoothingAlgorithm != SmoothingAlgorithms.None) {
          statements.Add(expr.Call("Settings_Interpolation", s.SnapMagnitude.Literal(), false.Literal()));
        }
      }

      if (c != null) {
        if (c.SmoothCorrection) {
          statements.Add(expr.Call("Settings_Interpolation", c.SnapMagnitude.Literal(), c.SmoothCorrection.Literal()));
        }
      }
    }

    public CodeExpression CreateFloatCompressionExpression(FloatCompression c, bool enabled) {
      if (c == null) {
        c = FloatCompression.Default();
      }

      if (enabled) {
        if (c.Enabled) {
          return "Bolt.PropertyFloatCompressionSettings.Create({0}, {1}f, {2}f, {3}f)".Expr(c.BitsRequired, c.Shift, c.Pack, c.Read);
        }
        else {
          return "Bolt.PropertyFloatCompressionSettings.Create()".Expr();
        }
      }
      else {
        return "default(Bolt.PropertyFloatCompressionSettings)".Expr();
      }
    }

    public List<CodeExpression> CreateAxisCompressionExpression(FloatCompression[] axes, AxisSelections selection) {
      if (axes == null) {
        selection = AxisSelections.XYZ;
      }

      List<CodeExpression> args = new List<CodeExpression>();
      args.Add(CreateFloatCompressionExpression(axes[Axis.X], (selection & AxisSelections.X) == AxisSelections.X));
      args.Add(CreateFloatCompressionExpression(axes[Axis.Y], (selection & AxisSelections.Y) == AxisSelections.Y));
      args.Add(CreateFloatCompressionExpression(axes[Axis.Z], (selection & AxisSelections.Z) == AxisSelections.Z));
      return args;
    }

    public void EmitFloatSettings(CodeExpression expr, CodeStatementCollection statements, FloatCompression c) {
      statements.Call(expr, "Settings_Float", CreateFloatCompressionExpression(c, true));
    }

    public void EmitVectorSettings(CodeExpression expr, CodeStatementCollection statements, FloatCompression[] axes, AxisSelections selection) {
      statements.Call(expr, "Settings_Vector", CreateAxisCompressionExpression(axes, selection).ToArray());
    }

    public void EmitQuaternionSettings(CodeExpression expr, CodeStatementCollection statements, FloatCompression[] axes, FloatCompression quaternion, AxisSelections selection) {
      if (axes == null || quaternion == null || selection == AxisSelections.XYZ) {
        statements.Call(expr, "Settings_Quaternion", CreateFloatCompressionExpression(quaternion, true));
      }
      else {
        statements.Call(expr, "Settings_QuaternionEuler", CreateAxisCompressionExpression(axes, selection).ToArray());
      }
    }

    public void EmitInitObject(string type, DomBlock block, Offsets offsets, params CodeExpression[] ctorArguments) {
      var tmp = block.Stmts.Var(type, block.TempVar());
      block.Stmts.Add(tmp.Assign(type.New(ctorArguments)));
      block.Stmts.Add(tmp.Call("Init", "obj".Expr().Field("Root"), "Bolt.NetworkObj_Meta.Offsets".New(offsets.OffsetProperties, offsets.OffsetStorage, offsets.OffsetObjects)));
    }

    public void EmitAddSettings(CodeExpression expr, CodeStatementCollection statements, Offsets offsets) {
      statements.Call(expr, "Settings_Property",
        Decorator.Definition.Name.Literal(),
        Decorator.Definition.Priority.Literal()
      );

      statements.Call(expr, "Settings_Offsets",
        offsets.OffsetProperties,
        offsets.OffsetStorage
      );

      // mecanim for states settings
      if ((Decorator.DefiningAsset is StateDecorator) && Decorator.Definition.PropertyType.MecanimApplicable) {
        var s = Decorator.Definition.StateAssetSettings;

        statements.Call(expr, "Settings_Mecanim",
          s.MecanimMode.Literal(),
          s.MecanimDirection.Literal(),
          s.MecanimDamping.Literal(),
          s.MecanimLayer.Literal()
        );
      }

      // collecting property specific settings
      AddSettings(expr, statements);
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
