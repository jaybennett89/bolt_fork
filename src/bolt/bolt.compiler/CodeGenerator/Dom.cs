using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public static class Dom {
    public static CodeExpression @value {
      get { return new CodeSnippetExpression("value"); }
    }

    public static CodeExpression @this {
      get { return new CodeThisReferenceExpression(); }
    }

    public static CodeStatement @return {
      get { return new CodeMethodReturnStatement(); }
    }

    public static CodeMemberProperty DeclareProperty(this CodeTypeDeclaration type, CodeTypeReference propertyType, string propertyName, Action<CodeStatementCollection> getter, Action<CodeStatementCollection> setter) {
      CodeMemberProperty prop;

      prop = new CodeMemberProperty();
      prop.Name = propertyName;
      prop.Type = propertyType;
      prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;

      if (prop.HasGet = (getter != null)) {
        getter(prop.GetStatements);
      }

      if (prop.HasSet = (setter != null)) {
        setter(prop.SetStatements);
      }

      type.Members.Add(prop);
      return prop;
    }

    public static CodeConstructor DeclareConstructor(this CodeTypeDeclaration type, Action<CodeConstructor> body) {
      CodeConstructor ctor;

      ctor = new CodeConstructor();
      ctor.Attributes = MemberAttributes.Public;
      type.Members.Add(ctor);

      body(ctor);

      return ctor;
    }

    public static CodeMemberField DeclareField(this CodeTypeDeclaration type, string fieldType, string fieldName) {
      CodeMemberField field;

      field = new CodeMemberField(fieldType, fieldName);
      field.Attributes = MemberAttributes.Private;

      type.Members.Add(field);

      return field;
    }

    public static void DeclareParameter(this CodeMemberMethod method, string paramType, string paramName) {
      method.Parameters.Add(new CodeParameterDeclarationExpression(paramType, paramName));
    }

    public static void Assign(this CodeStatementCollection stmts, CodeExpression left, CodeExpression right) {
      stmts.Add(new CodeAssignStatement(left, right));
    }

    public static CodeExpression Expr(this string expr) {
      return new CodeSnippetExpression(expr);
    }
  }
}
