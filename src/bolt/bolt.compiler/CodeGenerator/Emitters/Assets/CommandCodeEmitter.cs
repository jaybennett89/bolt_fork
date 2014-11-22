using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class CommandCodeEmitter : AssetCodeEmitter {
    public new CommandDecorator Decorator {
      get { return (CommandDecorator)base.Decorator; }
      set { base.Decorator = value; }
    }

    public void EmitTypes() {
      EmitClass();
      EmitInputInterface();
      EmitResultInterface();
      EmitFactoryClass();
    }

    void EmitPropertySetup(CodeTypeDeclaration type, string suffix, List<PropertyDecorator> properties) {

    }

    void EmitClass() {
      CodeTypeDeclaration type;

      type = Generator.DeclareClass(Decorator.Name);

      type.BaseTypes.Add("Bolt.Command");
      type.BaseTypes.Add(Decorator.InputInterfaceName);
      type.BaseTypes.Add(Decorator.ResultInterfaceName);

      type.DeclareProperty(Decorator.InputInterfaceName, "Input", get => get.Expr("return ({0})this", Decorator.InputInterfaceName));
      type.DeclareProperty(Decorator.ResultInterfaceName, "Result", get => get.Expr("return ({0})this", Decorator.ResultInterfaceName));

      type.DeclareField("Bolt.CommandMetaData", "_Meta").Attributes = MemberAttributes.Static;
      type.DeclareConstructorStatic(ctor => {
        ctor.Statements.Expr("_Meta = new Bolt.CommandMetaData()");
        ctor.Statements.Expr("_Meta.TypeId = new Bolt.TypeId({0})", Decorator.TypeId);
        ctor.Statements.Expr("_Meta.SmoothFrames = {0}", Decorator.Definition.SmoothFrames);
        ctor.Statements.Expr("PropertySetup_Input(_Meta.InputSerializers, new Stack<string>())");
        ctor.Statements.Expr("PropertySetup_Result(_Meta.ResultSerializers, new Stack<string>())");
      });

      EmitPropertySetup(type, "Input", Decorator.InputProperties);
      EmitPropertySetup(type, "Result", Decorator.ResultProperties);

      type.DeclareConstructor(ctor => {
        ctor.Attributes = MemberAttributes.Assembly;
        ctor.BaseConstructorArgs.Add("_Meta".Expr());
      });

      type.DeclareMethod(Decorator.InputInterfaceName, "Create", method => {
        method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
        method.Statements.Expr("return new {0}()", Decorator.Definition.Name);
      });

      type.DeclareProperty("Bolt.IProtocolToken", "Token", get => {
        get.Expr("return InputToken");
      }, set => {
        set.Expr("InputToken = value");
      }).PrivateImplementationType = new CodeTypeReference("Bolt.ICommandInput");

      type.DeclareProperty("Bolt.IProtocolToken", "Token", get => {
        get.Expr("return ResultToken");
      }, set => {
        set.Expr("ResultToken = value");
      }).PrivateImplementationType = new CodeTypeReference("Bolt.ICommandResult");

      for (int i = 0; i < Decorator.InputProperties.Count; ++i) {
        PropertyCodeEmitter.Create(Decorator.InputProperties[i]).EmitCommandMembers(type, new CodeSnippetExpression("this.InputData"), new CodeTypeReference(Decorator.InputInterfaceName), "_Input");
      }

      for (int i = 0; i < Decorator.ResultProperties.Count; ++i) {
        PropertyCodeEmitter.Create(Decorator.ResultProperties[i]).EmitCommandMembers(type, new CodeSnippetExpression("this.ResultData"), new CodeTypeReference(Decorator.ResultInterfaceName), "_Result");
      }
    }

    void EmitInputInterface() {
      CodeTypeDeclaration type = Generator.DeclareInterface(Decorator.InputInterfaceName);
      type.BaseTypes.Add("Bolt.ICommandInput");

      for (int i = 0; i < Decorator.InputProperties.Count; ++i) {
        PropertyCodeEmitter.Create(Decorator.InputProperties[i]).EmitSimpleIntefaceMember(type, true, true);
      }
    }

    void EmitResultInterface() {
      CodeTypeDeclaration type = Generator.DeclareInterface(Decorator.ResultInterfaceName);
      type.BaseTypes.Add("Bolt.ICommandResult");

      for (int i = 0; i < Decorator.ResultProperties.Count; ++i) {
        PropertyCodeEmitter.Create(Decorator.ResultProperties[i]).EmitSimpleIntefaceMember(type, true, true);
      }
    }

    void EmitFactoryClass() {
      CodeTypeDeclaration type;

      type = Generator.DeclareClass(Decorator.FactoryName);
      type.TypeAttributes = System.Reflection.TypeAttributes.NotPublic;
      type.BaseTypes.Add("Bolt.ICommandFactory");
      type.DeclareProperty("System.Type", "TypeObject", get => get.Expr("return typeof({0})", Decorator.Name));
      type.DeclareProperty("Bolt.TypeId", "TypeId", get => get.Expr("return new Bolt.TypeId({0})", Decorator.TypeId));

      type.DeclareProperty("Bolt.UniqueId", "TypeKey", get => {
        get.Expr("return new Bolt.UniqueId({0})", Decorator.Definition.Guid.ToByteArray().Join(", "));
      });

      type.DeclareMethod(typeof(object).FullName, "Create", method => method.Statements.Expr("return new {0}()", Decorator.Name));
    }
  }
}
