using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class CommandObjectDecorator : ObjectDecorator {
    public CommandObjectDecorator(StructDefinition def)
      : base(def) {
    }

    public override bool EmitAsInterface {
      get { return true; }
    }

    public override string BaseInterface {
      get { return "Bolt.INetworkCommandData"; }
    }

    public override string BaseClassMeta {
      get { return "Bolt.NetworkObj_Meta"; }
    }

    public override string BaseClass {
      get { return "Bolt.NetworkCommand_Data"; }
    }
  }

  public class CommandDecorator : AssetDecorator<CommandDefinition> {
    public override string FactoryInterface {
      get { return "Bolt.ICommandFactory"; }
    }

    public override string BaseClass {
      get { return "Bolt.Command"; }
    }

    public override bool EmitPropertyChanged {
      get { return false; }
    }

    public override List<PropertyDecorator> Properties {
      get;
      set;
    }

    public CommandDecorator(CommandDefinition def) {
      Definition = def;
    }
  }
}
