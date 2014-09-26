using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public struct StateProperty {
    public int Index;
    public PropertyDecorator Decorator;
  }

  public class StateDecorator : AssetDecorator<StateDefinition> {
    public int BitCount = 0;
    public StructDecorator RootStruct;
    public List<StateProperty> AllProperties = new List<StateProperty>();
    public List<PropertyDecorator> Properties = new List<PropertyDecorator>();

    public bool HasParent {
      get { return Definition.ParentGuid != Guid.Empty; }
    }

    public string InterfaceName {
      get { return "I" + Name; }
    }

    public string ClassName {
      get { return Name + "_State"; }
    }

    public string FactoryName {
      get { return Name + "_Factory"; }
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

    public List<StructDecorator> CalculateStructList() {
      return RootStruct.GetStructList(new List<StructDecorator>());
    }
  }
}
