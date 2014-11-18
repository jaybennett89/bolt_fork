using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  internal class SerializerGroup {
    static StringBuilder PathBuilder = new StringBuilder(1024);

    public int StorageCount;
    public int ObjectsCount;
    public HashSet<string> SerializerPaths = new HashSet<string>();
    public List<PropertySerializer> Serializers = new List<PropertySerializer>();

    public void AddSerializer(PropertySerializer serializer, int storage, int objects, Stack<string> path) {
      // setup properties
      serializer.Settings.PropertyPaths = new List<string>();
      serializer.Settings.OffsetStorage = StorageCount;
      serializer.Settings.OffsetObjects = ObjectsCount;
      serializer.Settings.OffsetSerializers = Serializers.Count;

      SetupPaths(serializer, path);

      // increment storage and add serializer
      StorageCount += storage;
      ObjectsCount += objects;

      Serializers.Add(serializer);
    }

    void SetupPaths(PropertySerializer serializer, Stack<string> path) {
      Assert.True(PathBuilder.Length == 0);

      path.Push(serializer.Settings.PropertyName);

      foreach (string p in path) {
        // add "." for each nesting
        if (PathBuilder.Length > 0) {
          PathBuilder.Append('.');
        }

        // add next path segment
        PathBuilder.Append(p);

        // create full path string
        serializer.Settings.PropertyPaths.Add(PathBuilder.ToString());

        // add to path set
        SerializerPaths.Add(serializer.Settings.PropertyPaths[serializer.Settings.PropertyPaths.Count - 1]);
      }

      // clear opath builder
      PathBuilder.Remove(0, PathBuilder.Length);

      path.Pop();
    }
  }
}
