using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bolt.Compiler {
  class EventCodeEmitter : AssetCodeEmitter {
    public new EventDecorator Decorator {
      get { return (EventDecorator)base.Decorator; }
      set { base.Decorator = value; }
    }

    public void EmitTypes() {
      EmitClass();
      EmitFactory();
      EmitListenerInterface();
    }

    void EmitListenerInterface() {
      CodeTypeDeclaration type;

      type = Generator.DeclareInterface(Decorator.ListenerName);
      type.DeclareMethod(typeof(void).FullName, "OnEvent", method => {
        method.DeclareParameter(Decorator.Name, "evnt");
      });
    }

    void EmitFactory() {
      CodeTypeDeclaration type;

      type = Generator.DeclareClass(Decorator.FactoryName);
      type.TypeAttributes = TypeAttributes.NotPublic;
      type.BaseTypes.Add("Bolt.IEventFactory");

      type.DeclareProperty("System.Type", "TypeObject", get => get.Expr("return typeof({0})", Decorator.Name));
      type.DeclareProperty("Bolt.TypeId", "TypeId", get => get.Expr("return new Bolt.TypeId({0})", Decorator.TypeId));

      type.DeclareProperty("Bolt.UniqueId", "TypeUniqueId", get => {
        get.Expr("return new Bolt.UniqueId({0})", Decorator.Definition.Guid.ToByteArray().Join(", "));
      });

      type.DeclareMethod(typeof(object).FullName, "Create", method => method.Statements.Expr("return new {0}()", Decorator.Name));
      type.DeclareMethod(typeof(void).FullName, "Dispatch", method => {
        method.DeclareParameter("Bolt.Event", "ev");
        method.DeclareParameter(typeof(object).FullName, "target");
        method.Statements.Expr("if (target is {0}) (({0})target).OnEvent(({1})ev)", Decorator.ListenerName, Decorator.Name);
      });
    }

    void EmitClass() {


      CodeTypeDeclaration type;

      type = Generator.DeclareClass(Decorator.Definition.Name);
      type.CommentSummary(cm => { cm.CommentDoc(Decorator.Definition.Comment ?? ""); });
      type.BaseTypes.Add("Bolt.Event");

      type.DeclareField("Bolt.EventMetaData", "_meta").Attributes = MemberAttributes.Static;
      type.DeclareConstructorStatic(ctor => {
        ctor.Statements.Expr("_meta.TypeId = new Bolt.TypeId({0})", Decorator.TypeId);
        ctor.Statements.Expr("_meta.ByteSize = {0}", Decorator.ByteSize);
        ctor.Statements.Expr("_meta.PropertySerializers = new Bolt.PropertySerializer[{0}]", Decorator.Properties.Count);

        for (int i = 0; i < Decorator.Properties.Count; ++i) {
          CodeExpression expr = PropertyCodeEmitter.Create(Decorator.Properties[i]).EmitEventPropertyInitializer();
          ctor.Statements.Assign("_meta.PropertySerializers[{0}]".Expr(i), expr);
        }
      });

      type.DeclareConstructor(ctor => {
        ctor.Attributes = MemberAttributes.Assembly;
        ctor.BaseConstructorArgs.Add("_meta".Expr());
      });

      type.DeclareMethod(Decorator.Definition.Name, "Raise", method => {
        method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
        method.DeclareParameter("BoltEntity", "entity");
        method.Statements.Expr("return Raise(entity, Bolt.EntityTargets.Everyone)");
      });

      type.DeclareMethod(typeof(string).FullName, "ToString", method => {
        method.Attributes = MemberAttributes.Public | MemberAttributes.Override;

        string name = Decorator.Definition.Name;
        string properties = Decorator.Properties.Select((p, i) => " " + p.Definition.Name + "={{" + i + "}}").Join("");
        string arguments = Decorator.Properties.Select((p, i) => ", this." + p.Definition.Name).Join("");
        method.Statements.Expr("return System.String.Format(\"[" + name + properties + "]\"" + arguments + ")");
      });

      type.DeclareMethod(Decorator.Definition.Name, "Raise", method => {
        method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
        method.DeclareParameter("BoltEntity", "entity");
        method.DeclareParameter("Bolt.EntityTargets", "targets");

        method.Statements.Expr("if (!entity) throw new System.ArgumentNullException(\"entity\")");
        method.Statements.Expr("if (!entity.isAttached) throw new BoltException(\"You can not raise events on entities which are not attached\")");

        switch (Decorator.Definition.EntitySenders) {
          case EntityEventSenders.OnlyOwner: method.Statements.Expr("if (!entity.isOwner) throw new BoltException(\"You are not the owner of {{0}}, you can not raise this event\", entity)"); break;
          case EntityEventSenders.OnlyController: method.Statements.Expr("if (!entity.hasControl) throw new BoltException(\"You are not the controller of {{0}}, you can not raise this event\", entity)"); break;
        }

        method.Statements.Expr("{0} evt", Decorator.Definition.Name);
        method.Statements.Expr("evt = new {0}()", Decorator.Definition.Name);
        method.Statements.Expr("evt.Targets = (byte) targets", Decorator.Definition.EntityTargets);
        method.Statements.Expr("evt.TargetEntity = entity.Entity");
        method.Statements.Expr("evt.IncrementRefs()");
        method.Statements.Expr("return evt");
      });

      type.DeclareMethod(Decorator.Definition.Name, "Raise", method => {
        method.Attributes = MemberAttributes.Private | MemberAttributes.Static;
        method.DeclareParameter(typeof(byte).FullName, "targets");
        method.DeclareParameter("BoltConnection", "connection");

        switch (Decorator.Definition.GlobalSenders) {
          case GlobalEventSenders.OnlyServer: method.Statements.Expr("if (!BoltCore.isServer) throw new BoltException(\"You are not the server of, you can not raise this event\")"); break;
          case GlobalEventSenders.OnlyClients: method.Statements.Expr("if (!BoltCore.isClient) throw new BoltException(\"You are not the a client, you can not raise this event\")"); break;
        }

        method.Statements.Expr("{0} evt", Decorator.Definition.Name);
        method.Statements.Expr("evt = new {0}()", Decorator.Definition.Name);
        method.Statements.Expr("evt.Targets = targets");
        method.Statements.Expr("evt.TargetConnection = connection");
        method.Statements.Expr("evt.IncrementRefs()");
        method.Statements.Expr("return evt");
      });

      type.DeclareMethod(Decorator.Definition.Name, "Raise", method => {
        method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
        method.DeclareParameter("Bolt.GlobalTargets", "targets");
        method.Statements.Expr("return Raise((byte) targets, null)");
      });

      type.DeclareMethod(Decorator.Definition.Name, "Raise", method => {
        method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
        method.DeclareParameter("BoltConnection", "connection");
        method.Statements.Expr("return Raise(Bolt.Event.GLOBAL_SPECIFIC_CONNECTION, connection)");
      });

      type.DeclareMethod(Decorator.Definition.Name, "Raise", method => {
        method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
        method.Statements.Expr("return Raise(Bolt.Event.GLOBAL_EVERYONE, null)");
      });

      for (int i = 0; i < Decorator.Properties.Count; ++i) {
        PropertyCodeEmitter.Create(Decorator.Properties[i]).EmitEventMembers(type);
      }
    }
  }
}
