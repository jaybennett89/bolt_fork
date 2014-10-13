using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterFloat : PropertyCodeEmitterSimple<PropertyDecoratorFloat> {
    public override string ReadMethod {
      get { return "ReadF32"; }
    }

    public override string PackMethod {
      get { return "PackF32"; }
    }

    public override string[] EmitSetPropertyDataArgument() {
      List<string> propertyData = new List<string>();

      if (Decorator.DefiningAsset is StateDecorator) {
        propertyData.Add(Decorator.Definition.StateAssetSettings.GetMecanimDataExpression());
      }

      //var c = Decorator.PropertyType.Compression;
      //if (c.Enabled && c.BitsRequired != 32) {
      //  propertyData.Add(string.Format("new Bolt.FloatCompression {{ Bits = {0}, Shift = {1}f, PackMultiplier = {2}f, ReadMultiplier = {3}f }}", c.BitsRequired, -c.MinValue, 1f / c.Accuracy, c.Accuracy));
      //}
      //else {
      //  propertyData.Add(string.Format("new Bolt.FloatCompression {{ Bits = 32 }}"));
      //}

      return propertyData.ToArray();
    }
  }
}
