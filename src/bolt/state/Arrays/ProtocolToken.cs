using Bolt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Bolt {
  [Documentation]
  public class NetworkArray_ProtocolToken : NetworkArray_Values<Bolt.IProtocolToken> {
    internal NetworkArray_ProtocolToken(int length, int stride)
      : base(length, stride) {
      Assert.True(stride == 1);
    }

    protected override Bolt.IProtocolToken GetValue(int index) {
      return Storage.Values[index].ProtocolToken;
    }

    protected override bool SetValue(int index, Bolt.IProtocolToken value) {
      if (ReferenceEquals(Storage.Values[index].ProtocolToken, value) == false) {
        Storage.Values[index].ProtocolToken = value;
        return true;
      }

      return false;
    }
  }
}