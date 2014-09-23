using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  public interface IStateFactory {
    Type TypeObject { get; }
    int TypeId { get; }

    IState Create();
  }
}
