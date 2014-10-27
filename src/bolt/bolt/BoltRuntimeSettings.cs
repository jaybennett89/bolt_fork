using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BoltEditorStartMode {
  None = 0,
  Server = 1,
  Client = 2
}

public class BoltRuntimeSettings : ScriptableObject {
  static BoltRuntimeSettings _instance;

  public static BoltRuntimeSettings instance {
    get {
      if (_instance == null) {
        _instance = (BoltRuntimeSettings)Resources.Load(typeof(BoltRuntimeSettings).Name, typeof(BoltRuntimeSettings));

        if (_instance == null) {
          BoltLog.Error("Could not find resource: '{0}' ", typeof(BoltRuntimeSettings));
        }
      }

      return _instance;
    }
  }

  [SerializeField]
  internal BoltConfig _config = new BoltConfig();

  [SerializeField]
  public int debugClientCount = 1;

  [SerializeField]
  public int debugStartPort = 54321;

  [SerializeField]
  public string debugStartMapName = null;

  [SerializeField]
  public bool debugPlayAsServer = false;

  [SerializeField]
  public BoltEditorStartMode debugEditorMode = BoltEditorStartMode.Server;

  [SerializeField]
  public KeyCode consoleToggleKey = KeyCode.Tab;

  [SerializeField]
  public bool consoleVisibleByDefault = true;

  [SerializeField]
  public int compilationWarnLevel = 4;

  [SerializeField]
  public bool allowStatePropertySetters = true;

  [SerializeField]
  public int editorSkin = 4;

  [SerializeField]
  public bool scopeModeHideWarningInGui = false;

  public BoltConfig GetConfigCopy() {
    return _config.Clone();
  }
}
