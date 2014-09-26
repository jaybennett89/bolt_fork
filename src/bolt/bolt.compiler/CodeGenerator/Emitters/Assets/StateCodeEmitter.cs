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

      foreach (PropertyDecorator property in Decorator.Properties) {
        if (property.DefiningAsset.Guid == Decorator.Guid) {
          PropertyCodeEmitter.Create(property).EmitStateInterfaceMembers(type);
        }
      }
    }

    public void EmitFactoryClass() {
      if (Decorator.Definition.IsAbstract) {
        return;
      }

      CodeTypeDeclaration type;

      type = Generator.DeclareClass(Decorator.FactoryName);
      type.TypeAttributes = TypeAttributes.NotPublic;
      type.BaseTypes.Add("Bolt.IStateFactory");

      type.DeclareProperty("Type", "Bolt.IStateFactory.TypeObject", get => {
        get.Expr("return typeof({0})", Decorator.InterfaceName);
      }).Attributes = default(MemberAttributes);

      type.DeclareProperty(typeof(int).FullName, "Bolt.IStateFactory.TypeId", get => {
        get.Expr("return {0}", Decorator.TypeId);
      }).Attributes = default(MemberAttributes);

      type.DeclareMethod("Bolt.IState", "Bolt.IStateFactory.Create", methoid => {
        methoid.Statements.Expr("return new {0}()", Decorator.ClassName);
      }).Attributes = default(MemberAttributes);
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

      type.DeclareField(typeof(int).FullName, "_FrameSize").Attributes = STATIC_PRIVATE;
      type.DeclareField(typeof(int).FullName, "_ObjectSize").Attributes = STATIC_PRIVATE;
      type.DeclareField(typeof(int).FullName, "_PropertyCount").Attributes = STATIC_PRIVATE;
      type.DeclareField("Bolt.PropertySerializer[]", "_Properties").Attributes = STATIC_PRIVATE;

      type.DeclareField("Bolt.BitArray", "_FullMask").Attributes = STATIC_PRIVATE;
      type.DeclareField("Bolt.BitArray", "_DiffMask").Attributes = STATIC_PRIVATE;

      type.DeclareField("Bolt.BitArray[]", "_Filters").Attributes = STATIC_PRIVATE;
      type.DeclareField("Dictionary<Bolt.Filter, Bolt.BitArray>", "_FilterPermutations").Attributes = STATIC_PRIVATE;
      type.DeclareField("Bolt.BitArray", "_ControllerFilter").Attributes = STATIC_PRIVATE;

      type.DeclareConstructorStatic(ctor => {
        ctor.Statements.Expr("_FrameSize = {0}", Decorator.RootStruct.ByteSize);
        ctor.Statements.Expr("_ObjectSize = {0}", Decorator.RootStruct.ObjectSize);
        ctor.Statements.Expr("_PropertyCount = {0}", Decorator.AllProperties.Count);

        ctor.Statements.Comment("default masks");
        ctor.Statements.Expr("_FullMask = Bolt.BitArray.CreateSet(_PropertyCount)");
        ctor.Statements.Expr("_DiffMask = Bolt.BitArray.CreateClear(_PropertyCount)");

        ctor.Statements.Comment("filters");
        ctor.Statements.Expr("_FilterPermutations = new Dictionary<Bolt.Filter, Bolt.BitArray>(128, Bolt.Filter.EqualityComparer.Instance)");
        ctor.Statements.Expr("_Filters = new Bolt.BitArray[32]");

        ctor.Statements.Comment("properties");
        ctor.Statements.Expr("_Properties = new Bolt.PropertySerializer[{0}]", Decorator.AllProperties.Count);

        EmitFilters(ctor);
        EmitControllerFilter(ctor);
        EmitProperties(ctor);
      });

      type.DeclareConstructor(ctor => {
        ctor.Comment("######## INSTANCE ########");
        ctor.BaseConstructorArgs.Add("_FrameSize".Expr());
        ctor.BaseConstructorArgs.Add("_ObjectSize".Expr());
        ctor.BaseConstructorArgs.Add("_PropertyCount".Expr());
        ctor.BaseConstructorArgs.Add(Decorator.Definition.PacketMaxBits.ToString().Expr());
        ctor.BaseConstructorArgs.Add(Decorator.Definition.PacketMaxProperties.ToString().Expr());
      });

      type.DeclareMethod("Bolt.PropertySerializer[]", "GetPropertyArray", method => {
        method.Statements.Expr("return _Properties");
      }).Attributes = (MemberAttributes.Override | MemberAttributes.Family);

      type.DeclareMethod("Bolt.BitArray", "GetFilterArray", method => {
        method.DeclareParameter("Bolt.Filter", "filter");
        method.Statements.Expr("return CalculateFilterPermutation(filter, _Filters, _FilterPermutations)");
      }).Attributes = (MemberAttributes.Override | MemberAttributes.Family);

      type.DeclareMethod("Bolt.BitArray", "GetControllerFilterArray", method => {
        method.Statements.Expr("return _ControllerFilter");
      }).Attributes = (MemberAttributes.Override | MemberAttributes.Family);

      type.DeclareMethod("Bolt.BitArray", "GetDiffArray", method => {
        method.Statements.Expr("return _DiffMask");
      }).Attributes = (MemberAttributes.Override | MemberAttributes.Family);

      type.DeclareMethod("Bolt.BitArray", "GetFullArray", method => {
        method.Statements.Expr("return _FullMask");
      }).Attributes = (MemberAttributes.Override | MemberAttributes.Family);

      foreach (PropertyDecorator p in Decorator.RootStruct.Properties) {
        PropertyCodeEmitter.Create(p).EmitStateClassMembers(Decorator, type);
      }
    }

    void EmitFilters(CodeTypeConstructor ctor) {
      ctor.Statements.Comment("init filters");
      foreach (FilterDefinition filter in Generator.Filters.OrderBy(x => x.Index)) {
        var ba = BitArray.CreateClear(Decorator.AllProperties.Count);

        for (int i = 0; i < Decorator.AllProperties.Count; ++i) {
          var p = Decorator.AllProperties[i];

          if ((p.Decorator.Definition.Filters & filter.Bit) == filter.Bit) {
            ba.Set(p.Index);
          }
        }

        ctor.Statements.Expr("_Filters[{0}] = Bolt.BitArray.CreateFrom({1}, new int[] {{ {2} }})", filter.Index.ToString().PadRight(2), Decorator.AllProperties.Count, ba.ToArray().Join(", "));
      }
    }

    void EmitControllerFilter(CodeTypeConstructor ctor) {
      ctor.Statements.Comment("init controller filter");
      var ba = BitArray.CreateClear(Decorator.AllProperties.Count);

      for (int i = 0; i < Decorator.AllProperties.Count; ++i) {
        var p = Decorator.AllProperties[i];

        if (!p.Decorator.Definition.ExcludeController) {
          ba.Set(p.Index);
        }
      }

      ctor.Statements.Expr("_ControllerFilter = Bolt.BitArray.CreateFrom({0}, new int[] {{ {1} }})", Decorator.AllProperties.Count, ba.ToArray().Join(", "));
    }

    void EmitProperties(CodeTypeConstructor ctor) {
      ctor.Statements.Comment("init properties");
      int byteOffset = 0;
      int objectOffset = 0;

      for (int i = 0; i < Decorator.AllProperties.Count; ++i) {
        // grab property
        var p = Decorator.AllProperties[i];
        var s = Generator.FindStruct(p.Decorator.DefiningAsset.Guid);

        // emit init expression
        ctor.Statements.Assign("_Properties[{0}]".Expr(p.Index.ToString().PadRight(4)), PropertyCodeEmitter.Create(p.Decorator).CreatePropertyArrayInitializerExpression(byteOffset, objectOffset));

        //ctor.Statements.Expr("_Properties[{0}] = new PropertySlice {{ Offset = {1}, Length = {2} }} /* {3}.{4} */", p.Index.ToString().PadRight(4), byteOffset.ToString().PadRight(5), p.Decorator.ByteSize.ToString().PadRight(3), s.Name, p.Decorator.Definition.Name);

        // increase byte offset
        byteOffset += p.Decorator.ByteSize;
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

