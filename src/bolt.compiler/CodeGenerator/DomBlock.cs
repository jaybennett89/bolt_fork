using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  public class DomBlock {
    int tmpVar = 0;
    string prefix = "";
    CodeStatementCollection stmts = null;

    public CodeStatementCollection Stmts {
      get { return stmts; }
    }

    public void Add(CodeExpression expression) {
      Stmts.Add(expression);
    }

    public void Add(CodeStatement statement) {
      Stmts.Add(statement);
    }

    public DomBlock(CodeStatementCollection stmts, string prefix) {
      this.stmts = stmts;
      this.prefix = prefix;
    }

    public DomBlock(CodeStatementCollection stmts)
      : this(stmts, "") {
    }

    public string TempVar() {
      return prefix + "tmp" + (tmpVar++);
    }
  }
}
