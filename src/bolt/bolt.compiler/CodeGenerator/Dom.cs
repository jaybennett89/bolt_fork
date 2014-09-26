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

    static void Comment(this CodeTypeMember member, bool doc, string comment, params object[] args) {
      member.Comments.Add(new CodeCommentStatement(string.Format(comment ?? "", args), doc));
    }

    public static void Comment(this CodeTypeMember member, string comment, params object[] args) {
      Comment(member, false, comment, args);
    }

    public static void CommentDoc(this CodeTypeMember member, string comment, params object[] args) {
      Comment(member, true, comment, args);
    }

    public static void Comment(this CodeStatementCollection statements, string comment, params object[] args) {
      statements.Add(new CodeCommentStatement(string.Format(comment, args)));
    }

    public static void CommentSummary(this CodeTypeMember member, Action<CodeTypeMember> commenter) {
      Comment(member, true, "<summary>");
      commenter(member);
      Comment(member, true, "</summary>");
    }

    public static CodeMemberMethod DeclareMethod(this CodeTypeDeclaration type, string returnType, string methodName, Action<CodeMemberMethod> body) {
      CodeMemberMethod method;

      method = new CodeMemberMethod();
      method.Name = methodName;
      method.ReturnType = new CodeTypeReference(returnType);
      method.Attributes = MemberAttributes.Public | MemberAttributes.Final;

      if (body != null) {
        body(method);
      }

      type.Members.Add(method);
      return method;
    }

    public static void DeclareMember(this CodeTypeDeclaration type, string memberSource, params object[] args) {
      type.Members.Add(new CodeSnippetTypeMember(string.Format(memberSource, args)));
    }

    public static CodeMemberProperty DeclareProperty(this CodeTypeDeclaration type, string propertyType, string propertyName, Action<CodeStatementCollection> getter) {
      return DeclareProperty(type, propertyType, propertyName, getter, null);
    }

    public static CodeMemberProperty DeclareProperty(this CodeTypeDeclaration type, string propertyType, string propertyName, Action<CodeStatementCollection> getter, Action<CodeStatementCollection> setter) {
      CodeMemberProperty prop;

      prop = new CodeMemberProperty();
      prop.Name = propertyName;
      prop.Type = new CodeTypeReference(propertyType);
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

    public static CodeTypeConstructor DeclareConstructorStatic(this CodeTypeDeclaration type, Action<CodeTypeConstructor> ctor) {
      CodeTypeConstructor method = new CodeTypeConstructor();
      type.Members.Add(method);

      ctor(method);

      return method;
    }

    public static CodeConstructor DeclareConstructor(this CodeTypeDeclaration type, Action<CodeConstructor> ctor) {
      CodeConstructor method;

      method = new CodeConstructor();
      method.Attributes = MemberAttributes.Public;
      type.Members.Add(method);

      ctor(method);

      return method;
    }

    public static CodeMemberField DeclareField(this CodeTypeDeclaration type, string fieldType, string fieldName) {
      CodeMemberField field;

      field = new CodeMemberField(fieldType, fieldName);
      field.Attributes = MemberAttributes.Assembly;

      type.Members.Add(field);

      return field;
    }

    public static void For(this CodeStatementCollection stmts, string variableName, string testExpression, Action<CodeStatementCollection> body) {
      CodeIterationStatement it;

      it = new CodeIterationStatement();
      it.InitStatement = (variableName + " = 0").Stmt();
      it.TestExpression = testExpression.Expr();
      it.IncrementStatement = ("++" + variableName).Stmt();

      body(it.Statements);

      stmts.Add(it);
    }

    public static void DeclareParameter(this CodeMemberProperty method, string paramType, string paramName) {
      method.Parameters.Add(new CodeParameterDeclarationExpression(paramType, paramName));
    }

    public static void DeclareParameter(this CodeMemberMethod method, string paramType, string paramName) {
      method.Parameters.Add(new CodeParameterDeclarationExpression(paramType, paramName));
    }

    public static void Assign(this CodeStatementCollection stmts, CodeExpression left, CodeExpression right) {
      stmts.Add(new CodeAssignStatement(left, right));
    }

    public static void Expr(this CodeStatementCollection stmts, string text, params object[] args) {
      stmts.Add(text.Expr(args));
    }

    public static void Stmt(this CodeStatementCollection stmts, string text, params object[] args) {
      stmts.Add(new CodeSnippetStatement(string.Format(text, args)));
    }

    public static CodeExpression Expr(this string text, params object[] args) {
      return new CodeSnippetExpression(string.Format(text, args));
    }

    public static CodeSnippetStatement Stmt(this string text, params object[] args) {
      return new CodeSnippetStatement(string.Format(text, args));
    }

    public static string Indent(this string text, int indent) {
      return (new string(' ', indent * 2) + text);
    }

    public static CodeExpression Expr(this int integer) {
      return new CodeSnippetExpression(integer.ToString());
    }
  }
}
