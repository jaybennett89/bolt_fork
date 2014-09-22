using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using System.Reflection;

namespace Bolt.Compiler {
  public class StateCodeEmitter : AssetCodeEmitter {
    public StateDecorator Decorator;

    public void Emit() {
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

      type.DeclareField(typeof(long[]).FullName, "completeMask").Attributes |= MemberAttributes.Static;
      type.DeclareField(typeof(long[][]).FullName, "filterMasks").Attributes |= MemberAttributes.Static;

      type.DeclareConstructorStatic(ctor => {
        var structs = Decorator.CalculateStructList();
        ctor.Statements.Expr("completeMask = new long[] {{ {0} }}", structs.Select(x => x.CalculateCompleteMask()).Join(", "));
        ctor.Statements.Expr("filterMasks = new long[32][]");

        foreach (PropertyFilterDefinition filter in Generator.Filters.OrderBy(x => x.Index)) {
          ctor.Statements.Expr("filterMasks[{0}] = new long[] {{ {1} }}", filter.Index, structs.Select(x => x.CalculateFilterMask(filter.Index)).Join(", "));
        }
      });

      type.DeclareConstructor(ctor => {
        ctor.BaseConstructorArgs.Add(Decorator.RootStruct.ByteSize.Expr());
        ctor.BaseConstructorArgs.Add(Decorator.RootStruct.StructCount.Expr());
        ctor.BaseConstructorArgs.Add("completeMask".Expr());
        ctor.BaseConstructorArgs.Add("filterMasks".Expr());
      });

      //type.DeclareMethod(typeof(long[]).FullName, "GetCompleteMask", method => {
      //  method.Statements.Expr("return CompleteMask");
      //}).Attributes = (MemberAttributes.Override | MemberAttributes.Family);

      //type.DeclareMethod(typeof(long[]).FullName, "GetFilterMask", method => {
      //  method.DeclareParameter("Bolt.Filter", "filter");
      //  method.Statements.Expr("return CalculatePermutation(filter, FilterMasks, FilterPermutations)");
      //}).Attributes = (MemberAttributes.Override | MemberAttributes.Family);

      //type.DeclareMethod("Bolt.Filter", "GetDefaultFilter", method => {
      //  method.Statements.Expr("return new Bolt.Filter({0})", 1 << Generator.Filters.First(x => x.IsDefault).Index);
      //}).Attributes = (MemberAttributes.Override | MemberAttributes.Family);
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
