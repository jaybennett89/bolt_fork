using Bolt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Bolt {
  [Documentation]
  public class NetworkArray_Entity : NetworkArray_Values<BoltEntity> {
    internal NetworkArray_Entity(int length, int stride)
      : base(length, stride) {
      Assert.True(stride == 1);
    }

    protected override BoltEntity GetValue(int index) {
      return BoltNetwork.FindEntity(Storage.Values[index].NetworkId);
    }

    protected override bool SetValue(int index, BoltEntity value) {
      NetworkId newValue = value == null ? new NetworkId() : value.Entity.NetworkId;

      if (Storage.Values[index].NetworkId != newValue) {
        Storage.Values[index].NetworkId = newValue;
        return true;
      }

      return false;
    }
  }
}