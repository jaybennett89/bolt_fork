using System.CodeDom;
using System.Reflection;

namespace Bolt.Compiler
{
  public class AssetCodeEmitter
  {
    public CodeTypeDeclaration MetaType;
    public CodeTypeDeclaration ObjectType;
    public CodeTypeDeclaration InterfaceType;

    public AssetDecorator Decorator;
    public CodeGenerator Generator { get { return Decorator.Generator; } }

    protected virtual void EmitObject()
    {
      ObjectType = Generator.DeclareClass(Decorator.Name);
      ObjectType.TypeAttributes = TypeAttributes.Public;
      ObjectType.BaseTypes.Add(Decorator.BaseClass);

      ObjectType.DeclareMethod(Decorator.Name, "Modify", method =>
      {
        method.DeclareModifyObsolete();
        method.Statements.Expr("return this");
      });

      ObjectType.DeclareConstructor(ctor =>
      {
        ctor.BaseConstructorArgs.Add(Decorator.NameMeta.Expr().Field("Instance"));
        EmitObjectCtor(ctor);
      });

      for (int i = 0; i < Decorator.Properties.Count; ++i)
      {
        PropertyCodeEmitter.Create(Decorator.Properties[i]).EmitObjectMembers(ObjectType);
      }
    }

    protected virtual void EmitObjectCtor(CodeConstructor ctor) { }

    protected virtual void EmitMeta()
    {
      MetaType = Generator.DeclareClass(Decorator.NameMeta);
      MetaType.TypeAttributes = TypeAttributes.NestedAssembly;
      MetaType.BaseTypes.Add(Decorator.BaseClassMeta);
      MetaType.BaseTypes.Add(Decorator.FactoryInterface);

      MetaType.DeclareField(Decorator.NameMeta, "Instance").Attributes = MemberAttributes.Static | MemberAttributes.Assembly;

      MetaType.DeclareConstructorStatic(ctor =>
      {
        ctor.Statements.Add("Instance".Expr().Assign(Decorator.NameMeta.New()));
        ctor.Statements.Add("Instance".Expr().Call("InitMeta"));

        EmitMetaStaticCtor(ctor);
      });

      // initialize object
      MetaType.DeclareMethod(typeof(void).FullName, "InitObject", method =>
      {
        method.Attributes = MemberAttributes.Assembly | MemberAttributes.Override;
        method.DeclareParameter("Bolt.NetworkObj", "obj");
        method.DeclareParameter("Bolt.NetworkObj_Meta.Offsets", "offsets");

        DomBlock block;
        block = new DomBlock(method.Statements);

        for (int i = 0; i < Decorator.Properties.Count; ++i)
        {
          block.Stmts.Comment("");
          block.Stmts.Comment("Property: " + Decorator.Properties[i].Definition.Name);
          PropertyCodeEmitter.Create(Decorator.Properties[i]).EmitObjectSetup(block);
        }

        EmitObjectInit(method);
      });

      // initialize meta class
      MetaType.DeclareMethod(typeof(void).FullName, "InitMeta", method =>
      {
        method.Attributes = MemberAttributes.Assembly | MemberAttributes.Override;

        DomBlock block;

        block = new DomBlock(method.Statements);
        block.Stmts.Comment("Setup fields");
        block.Stmts.Expr("this.TypeId = new Bolt.TypeId({0})", Decorator.TypeId);
        block.Stmts.Expr("this.CountStorage = {0}", Decorator.CountStorage);
        block.Stmts.Expr("this.CountObjects = {0}", Decorator.CountObjects);
        block.Stmts.Expr("this.CountProperties = {0}", Decorator.CountProperties);
        block.Stmts.Expr("this.Properties = new Bolt.NetworkPropertyInfo[{0}]", Decorator.CountProperties);

        EmitMetaInit(method);

        for (int i = 0; i < Decorator.Properties.Count; ++i)
        {
          block.Stmts.Comment("");
          block.Stmts.Comment("Property: " + Decorator.Properties[i].Definition.Name);
          PropertyCodeEmitter.Create(Decorator.Properties[i]).EmitMetaSetup(block);
        }
      });

      // emit factory interface
      EmitFactory();
    }

    protected virtual void EmitMetaStaticCtor(CodeTypeConstructor ctor) { }
    protected virtual void EmitMetaInit(CodeMemberMethod method) { }
    protected virtual void EmitObjectInit(CodeMemberMethod method) { }

    protected virtual void EmitFactory()
    {
      var factoryInterface = new CodeTypeReference(Decorator.FactoryInterface);

      MetaType.DeclareProperty("Bolt.TypeId", "TypeId", get =>
      {
        get.Expr("return TypeId");
      }).PrivateImplementationType = factoryInterface;

      MetaType.DeclareProperty("Bolt.UniqueId", "TypeKey", get =>
      {
        get.Expr("return new Bolt.UniqueId({0})", Decorator.Definition.Guid.ToByteArray().Join(", "));
      });

      MetaType.DeclareProperty("System.Type", "TypeObject", get =>
      {
        get.Expr("return typeof({0})", Decorator.Name);
      }).PrivateImplementationType = factoryInterface;

      MetaType.DeclareMethod(typeof(object).FullName, "Create", methoid =>
      {
        methoid.Statements.Expr("return new {0}()", Decorator.Name);
      }).PrivateImplementationType = factoryInterface;
    }

    public void Emit()
    {
      EmitObject();
      EmitMeta();
    }
  }
}
