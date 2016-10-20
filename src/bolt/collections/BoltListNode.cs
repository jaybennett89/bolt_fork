using Bolt;

[Documentation(Ignore = true)]
public interface IBoltListNode {
  object prev { get; set; }
  object next { get; set; }
  object list { get; set; }
}
