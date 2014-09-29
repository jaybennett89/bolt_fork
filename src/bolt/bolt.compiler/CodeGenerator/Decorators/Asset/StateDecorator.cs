using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class StateDecorator : AssetDecorator<StateDefinition> {
    public int BitCount = 0;
    public StructDecorator RootStruct;
    public List<StateDecoratorProperty> AllProperties = new List<StateDecoratorProperty>();
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
  }

  public struct StateDecoratorProperty {
    public int Index;
    public int Filters;

    public int OffsetBytes;
    public int OffsetObjects;

    public int[] CallbackIndices;
    public string[] CallbackPaths;

    public bool Controller;
    public PropertyDecorator Decorator;

    public string CallbackPathsExpression() {
      if (CallbackPaths == null || CallbackPaths.Length == 0 || CallbackPaths.Length == 1) {
        return "new string[0]";
      }

      return string.Format("new string[{0}] {{ {1} }}", CallbackPaths.Length - 1, CallbackPaths.Skip(1).Select(x => '"' + x + '"').Join(", "));
    }

    public string CreateIndicesExpr() {
      if (CallbackIndices == null || CallbackIndices.Length == 0) {
        return "new int[0]";
      }

      return string.Format("new int[{0}] {{ {1} }}", CallbackIndices.Length, CallbackIndices.Join(", "));
    }
  }
}
