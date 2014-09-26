using System.Collections.Generic;

class BoltCompilerOperation {
  public int eventIdOffset = 0;
  public string eventFilePath = "";
  public string networkFilePath = "";
  public string stateFilePath = "";
  public string prefabsFilePath = "";
  public string mecanimFilePath = "";
  public string mapsFilePath = "";
  public string commandsFilePath = "";
  public string assemblyInfoFilePath = "";
  public string projectFile = "";
  public Bolt.Compiler.Project project;
  public List<BoltPrefab> prefabs = new List<BoltPrefab>();
  public List<BoltEventAsset> events = new List<BoltEventAsset>();
  public List<BoltStateAsset> states = new List<BoltStateAsset>();
  public List<BoltMecanimAsset> mecanims = new List<BoltMecanimAsset>();
  public List<BoltCommandAsset> commands = new List<BoltCommandAsset>();
}
