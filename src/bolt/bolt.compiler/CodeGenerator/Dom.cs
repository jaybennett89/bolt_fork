using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public static class Dom {
    public static CodeMemberProperty DeclareProperty(this CodeTypeDeclaration type, CodeTypeReference propertyType, string name, Action<CodeStatementCollection> get, Action<CodeStatementCollection> set) {
      CodeMemberProperty prop;

      prop = new CodeMemberProperty();
      prop.Name = name;
      prop.Type = propertyType;
      prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;

      if (prop.HasGet = (get != null)) {
        get(prop.GetStatements);
      }

      if (prop.HasSet = (set != null)) {
        set(prop.SetStatements);
      }

      type.Members.Add(prop);
      return prop;
    }
  }
}
