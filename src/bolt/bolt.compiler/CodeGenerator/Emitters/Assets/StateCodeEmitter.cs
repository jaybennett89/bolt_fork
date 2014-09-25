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

    public override void EmitTypes() {
      EmitInterface();
      EmitClass();
    }

    void EmitInterface() {
      CodeTypeDeclaration type = Generator.DeclareInterface(Decorator.InterfaceName, CalulateInterfaceBaseTypes());

      foreach (PropertyDecorator property in Decorator.Properties) {
        if (property.DefiningAsset.Guid == Decorator.Guid) {
          PropertyCodeEmitter.Create(property).EmitStateInterfaceMembers(type);
        }
      }
    }

    void EmitClass() {
      CodeTypeDeclaration type;

      type = Generator.DeclareClass(Decorator.ClassName);
      type.TypeAttributes = TypeAttributes.NotPublic;
      type.BaseTypes.Add("Bolt.State");
      type.BaseTypes.Add(Decorator.InterfaceName);

      type.DeclareField(typeof(int).FullName, "_FrameSize").Attributes = MemberAttributes.Static | MemberAttributes.Private;
      type.DeclareField(typeof(int).FullName, "_PropertyCount").Attributes = MemberAttributes.Static | MemberAttributes.Private;
      type.DeclareField("PropertySlice[]", "_Properties").Attributes = MemberAttributes.Static | MemberAttributes.Private;

      type.DeclareField("Bolt.BitArray", "_FullMask").Attributes = MemberAttributes.Static | MemberAttributes.Private;
      type.DeclareField("Bolt.BitArray", "_DiffMask").Attributes = MemberAttributes.Static | MemberAttributes.Private;

      type.DeclareField("Bolt.BitArray[]", "_Filters").Attributes = MemberAttributes.Static | MemberAttributes.Private;
      type.DeclareField("Dictionary<Bolt.Filter, Bolt.BitArray>", "_FilterPermutations").Attributes = MemberAttributes.Static | MemberAttributes.Private;

      type.DeclareConstructorStatic(ctor => {
        var structs = Decorator.CalculateStructList();

        ctor.Statements.Expr("_FrameSize = {0}", Decorator.RootStruct.FrameSize);
        ctor.Statements.Expr("_PropertyCount = {0}", Decorator.AllProperties.Count);

        ctor.Statements.Comment("default masks");
        ctor.Statements.Expr("_FullMask = Bolt.BitArray.CreateSet(PropertyCount)");
        ctor.Statements.Expr("_DiffMask = Bolt.BitArray.CreateClear(PropertyCount)");

        ctor.Statements.Comment("filters");
        ctor.Statements.Expr("_FilterPermutations = new Dictionary<Bolt.Filter, Bolt.BitArray>(Bolt.Filter.EqualityComparer.Instance, 128)");
        ctor.Statements.Expr("_Filters = new Bolt.BitArray[32]");

        foreach (PropertyFilterDefinition filter in Generator.Filters.OrderBy(x => x.Index)) {
          var ba = BitArray.CreateClear(Decorator.AllProperties.Count);

          for (int i = 0; i < Decorator.AllProperties.Count; ++i) {
            var p = Decorator.AllProperties[i];

            if ((p.Decorator.Definition.StateAssetSettings.Filters & filter.Bit) == filter.Bit) {
              ba.Set(p.Index);
            }
          }

          ctor.Statements.Expr("_Filters[{0}] = Bolt.BitArray.CreateFrom({1}, new int[] {{ {2} }})", filter.Index, Decorator.AllProperties.Count, ba.ToArray().Join(", "));
        }

        ctor.Statements.Comment("properties");
        ctor.Statements.Expr("_Properties = new PropertySlice[{0}]", Decorator.AllProperties.Count);

        int byteOffset = 0;

        for (int i = 0; i < Decorator.AllProperties.Count; ++i) {
          // grab property
          var p = Decorator.AllProperties[i];
          var s = Generator.FindStruct(p.Decorator.DefiningAsset.Guid);

          // emit init expression
          ctor.Statements.Expr("_Properties[{0}] = new PropertySlice {{ Offset = {1}, Length = {2} }} /* {3}.{4} */", p.Index.ToString().PadRight(4), byteOffset.ToString().PadRight(5), p.Decorator.ByteSize.ToString().PadRight(3), s.Name, p.Decorator.Definition.Name);

          // increase byte offset
          byteOffset += p.Decorator.ByteSize;
        }
      });

      type.DeclareConstructor(ctor => {
        ctor.BaseConstructorArgs.Add("_FrameSize".Expr());
        ctor.BaseConstructorArgs.Add("_PropertyCount".Expr());
      });

      type.DeclareMethod("PropertySlice[]", "GetPropertyArray", method => {
        method.Statements.Expr("return _Properties");
      }).Attributes = (MemberAttributes.Override | MemberAttributes.Family);

      type.DeclareMethod("Bolt.BitArray", "GetFilterArray", method => {
        method.DeclareParameter("Bolt.Filter", "filter");
        method.Statements.Expr("return CalculatePermutation(filter, _Filters, _FilterPermutations)");
      }).Attributes = (MemberAttributes.Override | MemberAttributes.Family);

      type.DeclareMethod("Bolt.BitArray", "GetDiffArray", method => {
        method.Statements.Expr("return _DiffMask");
      }).Attributes = (MemberAttributes.Override | MemberAttributes.Family);

      type.DeclareMethod("Bolt.BitArray", "GetFullArray", method => {
        method.Statements.Expr("return _FullMask");
      }).Attributes = (MemberAttributes.Override | MemberAttributes.Family);
    }

    List<string> EmitByteMaskIndex(StructDecorator type) {
      int offset = 0;
      List<string> result = new List<string>();

      EmitByteMaskIndex(type, ref offset, result);

      return result;
    }

    void EmitByteMaskIndex(StructDecorator type, ref int offset, List<string> result) {

    }

    string[] CalulateInterfaceBaseTypes() {
      if (Decorator.HasParent) {
        return new string[] { Decorator.Parent.InterfaceName };
      }
      else {
        return new string[] { "IState" };
      }
    }
  }
}

