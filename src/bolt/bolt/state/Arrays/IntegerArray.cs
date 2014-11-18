using Bolt;
using System;

[Documentation]
public class IntegerArray : ValueArray<int> {
  internal IntegerArray(Bolt.State state, int offsetStorage, int offsetObjects, int length)
    : base(state, offsetStorage, offsetObjects, length) {
  }


  protected override int GetValue(int index) {
    return state.CurrentFrame.Storage[index].Int0;
  }

  protected override void SetValue(int index, int value) {
    state.CurrentFrame.Storage[index].Int0 = value;
  }
}
