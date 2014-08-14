using System;

public interface IBoltStateFactory {
  Type stateType { get; }
  IBoltState Create ();
}
