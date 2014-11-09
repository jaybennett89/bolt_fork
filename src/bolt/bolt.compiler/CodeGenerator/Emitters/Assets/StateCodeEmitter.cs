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
      type.DeclareField(Decorator.RootStruct.ModifierName, "_Modifier").Attributes = MemberAttributes.Assembly;

      type.DeclareConstructorStatic(ctor => {
        ctor.Statements.Expr("_Meta = new Bolt.State.StateMetaData()");

        ctor.Statements.Comment("Setup simple values");
        ctor.Statements.Expr("_Meta.TypeId = new Bolt.TypeId({0})", Decorator.TypeId);
        ctor.Statements.Expr("_Meta.FrameSize = {0}", Decorator.RootStruct.ByteSize);
        ctor.Statements.Expr("_Meta.ObjectCount = {0}", Decorator.RootStruct.ObjectSize);
        ctor.Statements.Expr("_Meta.PropertyCount = {0}", Decorator.AllProperties.Count);
        ctor.Statements.Expr("_Meta.PacketMaxBits = {0}", Decorator.Definition.PacketMaxBits);
        ctor.Statements.Expr("_Meta.PacketMaxProperties = {0}", Decorator.Definition.PacketMaxProperties);

        ctor.Statements.Comment("Setup data structures");

        ctor.Statements.Expr("_Meta.FramePool = new Bolt.State.FramePool(_Meta.FrameSize)");
        ctor.Statements.Expr("_Meta.PropertyFilters = new Bolt.BitArray[32]");
        ctor.Statements.Expr("_Meta.PropertyBlocks = new Bolt.Block[_Meta.PropertyCount]");
        ctor.Statements.Expr("_Meta.PropertyBlocksResult = new System.Int32[_Meta.PropertyCount]");
        ctor.Statements.Expr("_Meta.PropertyFilterCache = new Dictionary<Bolt.Filter, Bolt.BitArray>(128, Bolt.Filter.EqualityComparer.Instance)");
        ctor.Statements.Expr("_Meta.PropertySerializers = new Bolt.PropertySerializer[_Meta.PropertyCount]");
        ctor.Statements.Expr("_Meta.PropertyCallbackPaths = new HashSet<string>(new string[] {{ {0} }})", Decorator.AllProperties.SelectMany(x => x.CallbackPaths).Distinct().Select(x => '"' + x.Trim('.') + '"').Join(", "));

        EmitFilters(ctor);
        EmitProperties(ctor);
        EmitControllerFilter(ctor);
        EmitBlocks(ctor);
      });

      type.DeclareConstructor(ctor => {
        ctor.BaseConstructorArgs.Add("_Meta".Expr());
        ctor.Statements.Expr("_Modifier = new {0}(null, 0, 0)", Decorator.RootStruct.ModifierName);
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

    void EmitBlocks(CodeTypeConstructor ctor) {
      ctor.Statements.For("n", "n < _Meta.PropertyCount", body => {
        body.Expr("_Meta.PropertyBlocks[n] = new Bolt.Block {{ Offset = _Meta.PropertySerializers[n].Settings.ByteOffset, Length = (uint) _Meta.PropertySerializers[n].StateSettings.ByteLength }}");
      });
    }

    void DeclareModify(CodeTypeDeclaration type, StateDecorator decorator) {
      type.DeclareMethod(decorator.RootStruct.ModifierInterfaceName, "Modify", method => {
        method.PrivateImplementationType = new CodeTypeReference(decorator.InterfaceName);
        method.Statements.Expr("_Modifier.frame = Frames.first");
        method.Statements.Expr("return _Modifier");
      });
    }

    void EmitFilters(CodeTypeConstructor ctor) {
      ctor.Statements.Comment("Init Filters");
      foreach (FilterDefinition filter in Generator.Filters.OrderBy(x => x.Index)) {
        var ba = BitArray.CreateClear(Decorator.AllProperties.Count);

        for (int i = 0; i < Decorator.AllProperties.Count; ++i) {
          var p = Decorator.AllProperties[i];

          if ((p.Filters & filter.Bit) == filter.Bit) {
            ba.Set(p.Index);
          }
        }

        ctor.Statements.Expr("_Meta.PropertyFilters[{0}] = Bolt.BitArray.CreateFrom({1}, new int[] {{ {2} }})", filter.Index.ToString().PadRight(2), Decorator.AllProperties.Count, ba.ToArray().Join(", "));
      }
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

    void EmitProperties(CodeTypeConstructor ctor) {
      ctor.Statements.Comment("Init Serializers ");
      for (int i = 0; i < Decorator.AllProperties.Count; ++i) {
        // grab property
        var p = Decorator.AllProperties[i];
        var s = Generator.FindStruct(p.Decorator.DefiningAsset.Guid);

        var emitter = PropertyCodeEmitter.Create(p.Decorator);
        var initExpr = "_Meta.PropertySerializers[{0}]".Expr(p.Index.ToString().PadRight(4));

        // emit init expression
        ctor.Statements.Comment(p.PropertyPath);
        ctor.Statements.Assign(initExpr, emitter.GetCreateSerializerExpression());

        emitter.EmitAddSettings(initExpr, ctor.Statements, p);
      }
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

