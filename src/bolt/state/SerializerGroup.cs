using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  internal class SerializerGroup {
    static StringBuilder PathBuilder = new StringBuilder(1024);

    public int StorageCount;
    public int ObjectsCount;

    public BitSet[] Filters = new BitSet[32];
    public HashSet<string> Paths = new HashSet<string>();
    public List<PropertySerializer> Serializers = new List<PropertySerializer>();

    public SerializerGroup() {
      for (int i = 0; i < (Filters.Length - 1); ++i) {
        Filters[i] = BitSet.Full;
      }

      Assert.True(Filters[Filters.Length - 1].IsZero);
    }

    public void AddObject() {
      ObjectsCount += 1;
    }

    public void AddSerializer(PropertySerializer serializer, int storage, int objects, int filters, Stack<string> path) {
      // setup properties
      serializer.Settings.PropertyPaths = new List<string>();
      serializer.Settings.OffsetStorage = StorageCount;
      serializer.Settings.OffsetObjects = ObjectsCount;
      serializer.Settings.OffsetSerializers = Serializers.Count;

      // setup filters
      SetupFilters(serializer, filters);

      // calculate paths
      SetupPaths(serializer, path);

      // increment storage and add serializer
      StorageCount += storage;
      ObjectsCount += objects;

      Serializers.Add(serializer);
    }

    void SetupFilters(PropertySerializer serializer, int filters) {
      for (int i = 0; i < 32; ++i) {
        int b = 1 << i;
        if ((filters & b) == b) {
          Filters[i].Set(serializer.Settings.OffsetSerializers);
        }
      }
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
        Paths.Add(serializer.Settings.PropertyPaths[serializer.Settings.PropertyPaths.Count - 1]);
      }

      // clear opath builder
      PathBuilder.Remove(0, PathBuilder.Length);

      path.Pop();
    }
  }
}
