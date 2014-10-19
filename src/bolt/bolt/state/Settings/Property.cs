using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  internal struct PropertySettings {
    public int ByteOffset;
    public String PropertyName;
    public PropertyModes PropertyMode;

    public PropertySettings(int offset, string name, PropertyModes mode) {
      ByteOffset = offset;
      PropertyName = name;
      PropertyMode = mode;
    }
  }
}
