using Bolt;
using System;

[Documentation]
public class FloatArray : ValueArray<float> {
  internal FloatArray(Bolt.State state, int offsetStorage, int offsetObjects, int length)
    : base(state, offsetStorage, offsetObjects, length) {
  }

  protected override float GetValue(int index) {
    return state.CurrentFrame.Storage[index].Float0;
  }

  protected override void SetValue(int index, float value) {
    state.CurrentFrame.Storage[index].Float0 = value;
  }
}
