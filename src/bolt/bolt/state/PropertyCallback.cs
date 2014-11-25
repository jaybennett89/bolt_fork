using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  public delegate void PropertyCallback(IState state, string propertyPath, ArrayIndices arrayIndices);
  public delegate void PropertyCallbackSimple();
}
