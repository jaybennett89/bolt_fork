using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  internal class StateObjectGroup {
    public int StorageCount;
    public int ObjectsCount;
    public int SerializerCount;

    public State State;
    public NetworkObject[] Objects;

    public StateObjectGroup(State state) {
      State = state;
      Objects = new NetworkObject[State.MetaData.SerializerGroup.ObjectsCount];
    }

    public void AddObject(NetworkObject obj) {
      Assert.True(ObjectsCount < Objects.Length, "{0} < {1}", ObjectsCount, Objects.Length);
      Assert.Null(Objects[ObjectsCount]);

      obj.State = State;
      obj.OffsetStorage = StorageCount;
      obj.OffsetObjects = ObjectsCount;
      obj.OffsetSerializers = SerializerCount;

      // add object
      Objects[ObjectsCount] = obj;

      // increment count
      ObjectsCount += 1;
    }

    public void AddSerializer(int storage) {
      StorageCount += storage;
      SerializerCount += 1;
    }

    public NetworkObject this[int index] {
      get {
        if (index < 0 || index >= Objects.Length) {
          throw new IndexOutOfRangeException(index.ToString());
        }

        return Objects[index];
      }
      set { Objects[index] = value; }
    }
  }
}
