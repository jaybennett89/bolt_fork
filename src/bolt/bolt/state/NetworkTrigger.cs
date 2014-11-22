using System.Runtime.InteropServices;

namespace Bolt {
  [StructLayout(LayoutKind.Explicit)]
  struct NetworkTrigger {
    [FieldOffset(0)]
    public int Frame;

    [FieldOffset(4)]
    public int History;

    public void Update(int frame, bool set) {
      if (frame != this.Frame) {
        // must be larger than old frame
        Assert.True(frame > this.Frame);

        // get diff
        int diff = frame - this.Frame;

        // update history
        this.History = (diff < 32) ? (this.History << diff) : 0;

        if (set) {
          this.History |= 1;
        }

        this.Frame = frame;
      }
    }
  }
}
