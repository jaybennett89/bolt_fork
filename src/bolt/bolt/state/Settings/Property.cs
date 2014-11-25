using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  internal struct PropertySerializerSettings {
    public int OffsetStorage;
    public int OffsetObjects;
    public int OffsetSerializers;

    public ArrayIndices ArrayIndices;

    public int PropertyPriority;
    public String PropertyName;
    public List<String> PropertyPaths;
    public PropertyModes PropertyMode;

    public String PropertyFullPath {
      get { return PropertyPaths[PropertyPaths.Count - 1]; }
    }
  }
}
