using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class EventCodeEmitter : AssetCodeEmitter {
    public new EventDecorator Decorator {
      get { return (EventDecorator)base.Decorator; }
      set { base.Decorator = value; }
    }

    public void EmitEventClass() {


      CodeTypeDeclaration type;

      type = Generator.DeclareClass(Decorator.Definition.Name);
      type.CommentSummary(cm => { cm.CommentDoc(Decorator.Definition.Comment ?? ""); });
      type.BaseTypes.Add("Bolt.Event");

      type.DeclareConstructor(ctor => {
        ctor.Attributes = MemberAttributes.Private;
        ctor.BaseConstructorArgs.Add("default(Bolt.EventMetaData)".Expr());
      });

      if (Decorator.Definition.Global) {
        EmitGlobalMembers(type);
      }
      else {
        EmitEntityMembers(type);
      }
    }

    void EmitEntityMembers(CodeTypeDeclaration type) {
      type.DeclareMethod(Decorator.Definition.Name, "Raise", method => {
        method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
        method.DeclareParameter("BoltEntity", "entity");

        method.Statements.Expr("if (!entity) throw new System.ArgumentNullException(\"entity\")");
        method.Statements.Expr("if (!entity.isAttached) throw new BoltException(\"You can not raise events on entities which are not attached\")");

        switch (Decorator.Definition.EntitySenders) {
          case EntityEventSenders.OnlyOwner: method.Statements.Expr("if (!entity.isOwner) throw new BoltException(\"You are not the owner of {{0}}, you can not raise this event\", entity)"); break;
          case EntityEventSenders.OnlyController: method.Statements.Expr("if (!entity.hasControl) throw new BoltException(\"You are not the controller of {{0}}, you can not raise this event\", entity)"); break;
        }

        method.Statements.Expr("{0} evt", Decorator.Definition.Name);
        method.Statements.Expr("evt = new {0}()", Decorator.Definition.Name);
        method.Statements.Expr("evt.EntityTargets = Bolt.EntityTargets.{0}", Decorator.Definition.EntityTargets);
        method.Statements.Expr("evt.Entity = entity.Entity");
        method.Statements.Expr("return evt");
      });
    }

    void EmitGlobalRaiseMethod(CodeTypeDeclaration type, GlobalEventTargets target) {
      EmitGlobalRaiseMethod(type, target, null);
    }

    void EmitGlobalRaiseMethod(CodeTypeDeclaration type, GlobalEventTargets target, Action<CodeMemberMethod> methodBody) {
      type.DeclareMethod(Decorator.Definition.Name, string.Format("RaiseTo{0}", target), method => {
        method.Attributes = MemberAttributes.Public | MemberAttributes.Static;

        if (methodBody == null) {
          method.Statements.Expr("return Raise(Bolt.GlobalTargets.{0}, null)", target);
        }
        else {
          methodBody(method);
        }
      });
    }

    void EmitGlobalServerCheck(CodeMemberMethod method) {
      method.Statements.Expr("if (BoltCore.isClient) throw new BoltException(\"You are not the server, you can not raise this event\")"); 
    }

    void EmitGlobalClientCheck(CodeMemberMethod method) {
      method.Statements.Expr("if (BoltCore.isServer) throw new BoltException(\"You are not a client, you can not raise this event\")");
    }

    void EmitGlobalMembers(CodeTypeDeclaration type) {
      type.DeclareMethod(Decorator.Definition.Name, "Raise", method => {
        method.Attributes = MemberAttributes.Private | MemberAttributes.Static;
        method.DeclareParameter("Bolt.GlobalTargets", "targets");
        method.DeclareParameter("BoltConnection", "connection");

        switch (Decorator.Definition.GlobalSenders) {
          case GlobalEventSenders.OnlyServer: EmitGlobalServerCheck(method); break;
          case GlobalEventSenders.OnlyClients: EmitGlobalClientCheck(method); break;
        }

        method.Statements.Expr("{0} evt", Decorator.Definition.Name);
        method.Statements.Expr("evt = new {0}()", Decorator.Definition.Name);
        method.Statements.Expr("evt.Connection = connection");
        method.Statements.Expr("evt.GlobalTargets = targets");
        method.Statements.Expr("return evt");
      });

      if ((Decorator.Definition.GlobalTargets & GlobalEventTargets.Everyone) == GlobalEventTargets.Everyone) {
        EmitGlobalRaiseMethod(type, GlobalEventTargets.Everyone);
      }

      if ((Decorator.Definition.GlobalTargets & GlobalEventTargets.Others) == GlobalEventTargets.Others) {
        EmitGlobalRaiseMethod(type, GlobalEventTargets.Others);
      }

      if ((Decorator.Definition.GlobalTargets & GlobalEventTargets.AllClients) == GlobalEventTargets.AllClients) {
        EmitGlobalRaiseMethod(type, GlobalEventTargets.AllClients);
      }

      if ((Decorator.Definition.GlobalTargets & GlobalEventTargets.Server) == GlobalEventTargets.Server) {
        EmitGlobalRaiseMethod(type, GlobalEventTargets.Server, method => {
          EmitGlobalClientCheck(method);
          method.Statements.Expr("return Raise(Bolt.GlobalTargets.{0}, null)", GlobalEventTargets.Server);
        });
      }

      if ((Decorator.Definition.GlobalTargets & GlobalEventTargets.Client) == GlobalEventTargets.Client) {
        EmitGlobalRaiseMethod(type, GlobalEventTargets.Client, method => {
          method.DeclareParameter("BoltConnection", "connection");

          EmitGlobalServerCheck(method);

          method.Statements.Expr("if (connection == null) throw new System.ArgumentNullException(\"connection\")");
          method.Statements.Expr("return Raise(Bolt.GlobalTargets.{0}, connection)", GlobalEventTargets.Client);
        });
      }
    }
  }
}
