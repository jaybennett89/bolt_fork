using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  struct PropertyMecanimData {
    public MecanimMode Mode;
    public MecanimDirection OwnerDirection;
    public MecanimDirection ControllerDirection;
    public MecanimDirection OthersDirection;
    public int Layer;
    public bool Enabled;
    public float Damping;
  }
}
