using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class StateDecorator : AssetDecorator<StateDefinition> {
    public bool HasParent {
      get { return Definition.ParentGuid != Guid.Empty; }
    }

    public override string FactoryInterface {
      get { return "Bolt.ISerializerFactory"; }
    }

    public override List<PropertyDecorator> Properties {
      get;
      set;
    }

    public StateDecorator Parent {
      get { return Generator.FindState(Definition.ParentGuid); }
    }

    public IEnumerable<StateDecorator> ParentList {
      get {
        if (HasParent) {
          var parent = Generator.FindState(Definition.ParentGuid);

          foreach (StateDecorator def in parent.ParentList) {
            yield return def;
          }

          yield return parent;
        }

        yield break;
      }
    }

  }
}
