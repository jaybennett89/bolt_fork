using Bolt;

[Documentation(Ignore = true)]
public struct STuple<T0, T1> {
  public readonly T0 item0;
  public readonly T1 item1;

  public STuple (T0 a, T1 b) {
    item0 = a;
    item1 = b;
  }
}
