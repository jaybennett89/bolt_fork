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

      type.DeclareField(typeof(int).FullName, "FrameSize").Attributes |= MemberAttributes.Static;
      type.DeclareField(typeof(int).FullName, "StructCount").Attributes |= MemberAttributes.Static;
      type.DeclareField("ByteMask[]", "ByteMasks").Attributes |= MemberAttributes.Static;

      type.DeclareField(typeof(long[]).FullName, "FullMask").Attributes |= MemberAttributes.Static;
      type.DeclareField(typeof(long[]).FullName, "DiffMask").Attributes |= MemberAttributes.Static;
      type.DeclareField(typeof(long[][]).FullName, "Filters").Attributes |= MemberAttributes.Static;
      type.DeclareField("Dictionary<Filter, long[]>", "FilterPermutations").Attributes |= MemberAttributes.Static;

      type.DeclareConstructorStatic(ctor => {
        var structs = Decorator.CalculateStructList();

        ctor.Statements.Expr("FrameSize = {0}", Decorator.RootStruct.FrameSize);
        ctor.Statements.Expr("StructCount = {0}", Decorator.RootStruct.StructCount);

        ctor.Statements.Expr("// default masks");
        ctor.Statements.Expr("FullMask = new long[StructCount] {{ {0} }}", structs.Select(x => x.CalculateCompleteMask()).Join(", "));
        ctor.Statements.Expr("DiffMask = new long[StructCount]");

        ctor.Statements.Expr("// filters");
        ctor.Statements.Expr("FilterPermutations = new Dictionary<Bolt.Filter, long[]>(Bolt.Filter.EqualityComparer.Instance, 512)");
        ctor.Statements.Expr("Filters = new long[32][]");

        foreach (PropertyFilterDefinition filter in Generator.Filters.OrderBy(x => x.Index)) {
          ctor.Statements.Expr("Filters[{0}] = new long[StructCount] {{ {1} }}", filter.Index, structs.Select(x => x.CalculateFilterMask(filter.Index)).Join(", "));
        }

        ctor.Statements.Expr("// byte masks");
        ctor.Statements.Expr("int stuctIndex = 0");
        ctor.Statements.Expr("int byteOffset = 0");
        ctor.Statements.Expr("ByteMasks = new ByteMask[FrameSize]");
        ctor.Statements.Expr("{0}.InitByteMasks(ByteMasks, ref stuctIndex, ref byteOffset)", Decorator.RootStruct.Name);
        ctor.Statements.Expr("Assert.True(stuctIndex == StructCount)");
        ctor.Statements.Expr("Assert.True(byteOffset == FrameSize)");
      });

      type.DeclareConstructor(ctor => {
        ctor.BaseConstructorArgs.Add("FrameSize".Expr());
        ctor.BaseConstructorArgs.Add("StructCount".Expr());
      });

      type.DeclareMethod(typeof(long[]).FullName, "GetFilter", method => {
        method.DeclareParameter("Bolt.Filter", "filter");
        method.Statements.Expr("return CalculatePermutation(filter, Filters, FilterPermutations)");
      }).Attributes = (MemberAttributes.Override | MemberAttributes.Family);

      type.DeclareMethod(typeof(long[]).FullName, "GetDiffMask", method => {
        method.Statements.Expr("return DiffMask");
      }).Attributes = (MemberAttributes.Override | MemberAttributes.Family);

      type.DeclareMethod(typeof(long[]).FullName, "GetFullMask", method => {
        method.Statements.Expr("return FullMask");
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

