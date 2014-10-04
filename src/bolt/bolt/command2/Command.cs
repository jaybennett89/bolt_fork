using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  internal struct CommandMetaData {

  }

  public abstract class Command {
    internal CommandMetaData Meta;
    internal Command(CommandMetaData meta) {
      Meta = meta;
    }
  }
}
