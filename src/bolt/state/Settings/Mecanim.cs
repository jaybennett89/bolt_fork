using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  struct PropertyMecanimSettings {
    public MecanimMode Mode;
    public MecanimDirection Direction;
    public int Layer;
    public float Damping;

    public bool Enabled {
      get { return Mode != MecanimMode.Disabled; }
    }
  }
}
