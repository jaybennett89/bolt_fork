using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  interface IEventInfo {
    TypeId TypeId { get; }
    PropertySerializer[] Properties { get; }
  }
}
