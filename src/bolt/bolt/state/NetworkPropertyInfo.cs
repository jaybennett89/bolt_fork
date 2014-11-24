using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  internal class NetworkPropertyPath : IEnumerable<string> {
    internal string Name;
    internal NetworkPropertyPath Next;

    public IEnumerator<string> GetEnumerator() {
      NetworkPropertyPath current = this;

      while (current != null) {
        yield return current.Name;
        current = current.Next;
      }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }
  }

  internal struct NetworkPropertyInfo {
    public int OffsetObjects;
    public NetworkProperty Property;
    public NetworkPropertyPath Path;
  }
}
