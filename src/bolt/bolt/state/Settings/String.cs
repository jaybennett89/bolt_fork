using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  internal struct PropertyStringSettings {
    public StringEncodings Encoding;
    public Encoding EncodingClass {
      get {
        switch (Encoding) {
          case StringEncodings.ASCII: return System.Text.Encoding.ASCII;
          case StringEncodings.UTF8: return System.Text.Encoding.UTF8;
        }

        throw new NotSupportedException();
      }
    }
  }
}
