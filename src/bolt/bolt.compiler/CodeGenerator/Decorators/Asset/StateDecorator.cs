using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
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
  }

  public class StateProperty {

    public int Index { get; private set; }
    public int Filters { get; private set; }
    public bool Controller { get; private set; }

    public int OffsetBytes { get; private set; }
    public int OffsetObjects { get; private set; }

    public int[] CallbackIndices { get; private set; }
    public string[] CallbackPaths { get; private set; }

    public string PropertyPath {
      get { return CallbackPaths[CallbackPaths.Length - 1]; }
    }

    public PropertyDecorator Decorator { get; private set; }

    public StateProperty() {
      Filters = -1;
      Controller = true;

      CallbackPaths = new string[0];
      CallbackIndices = new int[0];
    }

    public StateProperty Combine(int index, PropertyDecorator decorator) {
      Assert.Null(Decorator);

      StateProperty clone;

      clone = Clone();
      clone.Index = index;
      clone.Decorator = decorator;

      return clone;
    }

    public StateProperty Combine(int filters, bool controller) {
      StateProperty clone;

      clone = Clone();
      clone.Filters = clone.Filters & filters;
      clone.Controller = clone.Controller && controller;

      return clone;
    }

    public StateProperty Combine(int offsetBytes, int offsetObjects) {
      StateProperty clone;

      clone = Clone();
      clone.OffsetBytes = offsetBytes;
      clone.OffsetObjects = offsetObjects;

      return clone;
    }

    public StateProperty AddIndex(int index) {
      StateProperty clone;

      clone = Clone();
      clone.CallbackIndices = clone.CallbackIndices.Add(index);

      return clone;
    }

    public StateProperty AddCallbackPath(string extend) {
      StateProperty clone = Clone();

      if (clone.CallbackPaths.Length == 0) {
        clone.CallbackPaths = new string[] { extend };
      }
      else { 
        string prev = clone.CallbackPaths[clone.CallbackPaths.Length - 1];
        clone.CallbackPaths = clone.CallbackPaths.Add(prev + "." + extend);
      }

      return clone;
    }

    public string CallbackPathsExpression() {
      if (CallbackPaths == null) {
        return "new string[0]";
      }
      else {
        return string.Format("new string[{0}] {{ {1} }}", CallbackPaths.Length, CallbackPaths.Select(x => '"' + x + '"').Join(", "));
      }
    }

    public string CallbackIndicesExpression() {
      if (CallbackIndices == null) {
        return "new Bolt.ArrayIndices(new int[0])";
      }
      else {
        return string.Format("new Bolt.ArrayIndices(new int[{0}] {{ {1} }})", CallbackIndices.Length, CallbackIndices.Join(", "));
      }
    }

    StateProperty Clone() {
      return (StateProperty)MemberwiseClone();
    }
  }
}
