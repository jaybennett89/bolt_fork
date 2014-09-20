using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class StateDecorator : AssetDecorator<StateDefinition> {
    public int BitCount = 0;
    public StructDecorator RootStruct;
    public List<PropertyDecorator> Properties = new List<PropertyDecorator>();

    public bool HasParent {
      get { return Definition.ParentGuid != Guid.Empty; }
    }

    public string Name {
      get { return Path.GetFileNameWithoutExtension(Definition.AssetPath); }
    }

    public string InterfaceName {
      get { return "I" + Name; }
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

    public void CloneProperties(StateDecorator from) {
      foreach (PropertyDefinition d in from.Definition.Properties) {
        if (d.Enabled) {
          PropertyDecorator decorator;

          decorator = new PropertyDecorator();
          decorator.Generator = Generator;
          decorator.Definition = d;
          decorator.DefiningAsset = from;

          Properties.Add(decorator);
        }
      }
    }
  }
}
