using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  internal struct PropertySerializerSettings {
    public int OffsetStorage;
    public int OffsetObjects;
    public int OffsetSerializers;

    public int PropertyPriority;
    public String PropertyName;
    public List<String> PropertyPaths;
    public PropertyModes PropertyMode;

    public String PropertyFullPath {
      get { return PropertyPaths[PropertyPaths.Count - 1]; }
    }
  }

  internal struct PropertySettings {
    public int SerializerOffset;
    public int ByteOffset;
    public String PropertyName;
    public PropertyModes PropertyMode;

    public PropertySettings(int offset, string name, PropertyModes mode) {
      ByteOffset = SerializerOffset = offset;
      PropertyName = name;
      PropertyMode = mode;
    }
  }
}
