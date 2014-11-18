using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using System.Reflection;

namespace Bolt.Compiler {
  public class StateCodeEmitter : AssetCodeEmitter {
    public new StateDecorator Decorator {
      get { return (StateDecorator)base.Decorator; }
      set { base.Decorator = value; }
    }

    public void EmitInterface() {
      CodeTypeDeclaration type = Generator.DeclareInterface(Decorator.InterfaceName, CalulateInterfaceBaseTypes());
      type.CommentSummary(cm => { cm.CommentDoc(Decorator.Definition.Comment ?? ""); });

      foreach (PropertyDecorator property in Decorator.Properties) {
        if (property.DefiningAsset.Guid == Decorator.Guid) {
          PropertyCodeEmitter.Create(property).EmitStateInterfaceMembers(type);
        }
      }

      type.DeclareMethod(Decorator.RootStruct.ModifierInterfaceName, "Modify", method => {
        method.DeclareModifyObsolete();

        if (Decorator.HasParent) {
          method.Attributes = MemberAttributes.New;
        }
      });
    }

    public void EmitFactoryClass() {
      if (Decorator.Definition.IsAbstract) {
        return;
      }

      CodeTypeDeclaration type;

      type = Generator.DeclareClass(Decorator.FactoryName);
      type.TypeAttributes = TypeAttributes.NotPublic;
      type.BaseTypes.Add("Bolt.ISerializerFactory");

      type.DeclareProperty("Type", "TypeObject", get => {
        get.Expr("return typeof({0})", Decorator.InterfaceName);
      });

      type.DeclareProperty("Bolt.TypeId", "TypeId", get => {
        get.Expr("return new Bolt.TypeId({0})", Decorator.TypeId);
      });

      type.DeclareProperty("Bolt.UniqueId", "TypeUniqueId", get => {
        get.Expr("return new Bolt.UniqueId({0})", Decorator.Definition.Guid.ToByteArray().Join(", "));
      });

      type.DeclareMethod(typeof(object).FullName, "Create", methoid => {
        methoid.Statements.Expr("return new {0}()", Decorator.ClassName);
      });
    }

    public void EmitImplementationClass() {
      const MemberAttributes STATIC_PRIVATE = MemberAttributes.Static | MemberAttributes.Private;

      if (Decorator.Definition.IsAbstract) {
        return;
      }

      CodeTypeDeclaration type;

      type = Generator.DeclareClass(Decorator.ClassName);
      type.TypeAttributes = TypeAttributes.NotPublic;
      type.BaseTypes.Add("Bolt.State");
      type.BaseTypes.Add(Decorator.InterfaceName);

      type.DeclareField("Bolt.State.StateMetaData", "_Meta").Attributes = STATIC_PRIVATE;
      type.DeclareField(Decorator.RootStruct.Name, "_Root").Attributes = MemberAttributes.Assembly;

      type.DeclareConstructorStatic(ctor => {
        ctor.Statements.Comment("Setup Meta Object");
        ctor.Statements.Expr("_Meta = new Bolt.State.StateMetaData()");
        ctor.Statements.Expr("_Meta.TypeId = new Bolt.TypeId({0})", Decorator.TypeId);
        ctor.Statements.Expr("_Meta.PacketMaxBits = {0}", Decorator.Definition.PacketMaxBits);
        ctor.Statements.Expr("_Meta.PacketMaxProperties = {0}", Decorator.Definition.PacketMaxProperties);

        ctor.Statements.Comment("Setup Properties");
        ctor.Statements.Expr("{0}.PropertySetup(_Meta.Serializers)", Decorator.RootStruct.Name);

        EmitControllerFilter(ctor);

        EmitCallbacks(ctor, "OnRender", p => p.Decorator.OnRenderCallback);
        EmitCallbacks(ctor, "OnSimulateAfter", p => p.Decorator.OnSimulateAfterCallback);
        EmitCallbacks(ctor, "OnSimulateBefore", p => p.Decorator.OnSimulateBeforeCallback);
      });

      type.DeclareConstructor(ctor => {
        ctor.BaseConstructorArgs.Add("_Meta".Expr());
        ctor.Statements.Expr("_Root = new {0}()", Decorator.RootStruct.Name);
        ctor.Statements.Expr("_Root.State = this", Decorator.RootStruct.Name);
        ctor.Statements.Expr("_Root.OffsetObjects = 0");
        ctor.Statements.Expr("_Root.OffsetStorage = 0");
        ctor.Statements.Expr("_Root.OffsetSerializers = 0");
      });

      type.DeclareMethod(typeof(string).FullName, "ToString", method => {
        method.Statements.Expr(string.Format(@"return string.Format(""[Serializer {0}]"")", Decorator.InterfaceName));
      }).Attributes = MemberAttributes.Override | MemberAttributes.Public;

      foreach (PropertyDecorator p in Decorator.RootStruct.Properties) {
        PropertyCodeEmitter.Create(p).EmitStateMembers(Decorator, type);
      }

      foreach (StateDecorator parent in Decorator.ParentList) {
        DeclareModify(type, parent);
      }

      DeclareModify(type, Decorator);
    }

    void EmitCallbacks(CodeTypeConstructor ctor, string array, Func<StateProperty, bool> check) {
      int n = 0;

      foreach (var sp in Decorator.AllProperties.Where(check)) {
        ctor.Statements.Expr("_Meta.PropertySerializers{0}[{1}] = _Meta.PropertySerializers[{2}]", array, n, sp.Index);
        n += 1;
      }
    }

    void DeclareModify(CodeTypeDeclaration type, StateDecorator decorator) {
      type.DeclareMethod(decorator.RootStruct.Name, "Modify", method => {
        method.PrivateImplementationType = new CodeTypeReference(decorator.InterfaceName);
        method.DeclareModifyObsolete();
        method.Statements.Expr("return _Root");
      });
    }

    void EmitControllerFilter(CodeTypeConstructor ctor) {
      ctor.Statements.Comment("Init Controller Filter");
      var ba = BitArray.CreateClear(Decorator.AllProperties.Count);

      for (int i = 0; i < Decorator.AllProperties.Count; ++i) {
        var p = Decorator.AllProperties[i];
        if (p.Controller) {
          ba.Set(p.Index);
        }
      }

      ctor.Statements.Expr("_Meta.PropertyControllerFilter = Bolt.BitArray.CreateFrom({0}, new int[] {{ {1} }})", Decorator.AllProperties.Count, ba.ToArray().Join(", "));
    }

    string[] CalulateInterfaceBaseTypes() {
      if (Decorator.HasParent) {
        return new string[] { Decorator.Parent.InterfaceName };
      }
      else {
        return new string[] { "Bolt.IState" };
      }
    }
  }
}


