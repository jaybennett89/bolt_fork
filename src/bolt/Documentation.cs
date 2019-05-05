using System;

namespace Bolt {
  [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
  public sealed class DocumentationAttribute : Attribute {
    public string Alias {
      get;
      set;
    }

    public bool Ignore {
      get;
      set;
    }
  }
}
