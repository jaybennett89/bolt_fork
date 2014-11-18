using Bolt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

[Documentation]
public class EntityArray : ValueArray<BoltEntity> {
  internal EntityArray(Bolt.State state, int offsetStorage, int offsetObjects, int length)
    : base(state, offsetStorage, offsetObjects, length) {
  }

  protected override BoltEntity GetValue(int index) {
    return BoltCore.FindEntity(state.CurrentFrame.Storage[index].NetworkId).UnityObject;
  }

  protected override void SetValue(int index, BoltEntity value) {
    if (value == null) {
      state.CurrentFrame.Storage[index].NetworkId = new NetworkId();
    }

    state.CurrentFrame.Storage[index].NetworkId = value.Entity.NetworkId;
  }
}
