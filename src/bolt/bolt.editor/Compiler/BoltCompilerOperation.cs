using System.Collections.Generic;

class BoltCompilerOperation {
  public int eventIdOffset = 0;
  public string networkFilePath = "";
  public string prefabsFilePath = "";
  public string scenesFilePath = "";
  public string assemblyInfoFilePath = "";
  public string projectFilePath = "";
  public Bolt.Compiler.Project project;
  public List<BoltPrefab> prefabs = new List<BoltPrefab>();
}
