using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

static class BoltEditorUtils {
  public static bool hasPro {
    get { return UnityEditorInternal.InternalEditorUtility.HasPro(); }
  }

  public static string EnumFlagsToString (this Enum value) {
    return value.GetType().CSharpName() + "." + value.ToString().Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).Join(" | " + value.GetType() + ".");
  }

  public static string CSharpIdentifier (this string value) {
    if (Char.IsDigit(value[0])) {
      value = "_" + value;
    }

    return Regex.Replace(value, "[^a-zA-Z0-9_]+", "_");
  }

  public static string MakePath (params string[] parts) {
    return String.Join(Path.DirectorySeparatorChar.ToString(), parts.Select(x => x.TrimEnd('/', '\\').TrimStart('\\')).ToArray()).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
  }

  public static string MakeAssetPath (params string[] parts) {
    string path;

    path = MakePath(parts);
    path = path.Replace(MakePath(Application.dataPath), "");
    path = MakePath("Assets", path);

    return path;

  }

  public static string Join<T> (this IEnumerable<T> items, string seperator) {
    return String.Join(seperator, items.Select(x => x.ToString()).ToArray());
  }

  public static string GetResourceText (string path) {
    return Assembly.GetExecutingAssembly().GetResourceText(path);
  }

  public static string GetResourceText (this Assembly asm, string path) {
    using (Stream s = asm.GetManifestResourceStream(path)) {
      using (StreamReader r = new StreamReader(s)) {
        return r.ReadToEnd();
      }
    }
  }

  public static byte[] GetResourceBytes (string path) {
    return Assembly.GetExecutingAssembly().GetResourceBytes(path);
  }

  public static byte[] GetResourceBytes (this Assembly asm, string path) {
    using (var stream = asm.GetManifestResourceStream(path)) {
      byte[] buffer = new byte[stream.Length];
      stream.Read(buffer, 0, buffer.Length);
      return buffer;
    }
  }

  public static IEnumerable<T> FindAssets<T> () where T : ScriptableObject {
    foreach (var file in Directory.GetFiles(@"Assets", "*.asset", SearchOption.AllDirectories)) {
      T asset = AssetDatabase.LoadAssetAtPath(file, typeof(T)) as T;

      if (asset) {
        yield return asset;
      }
    }
  }

  public static ScriptableObject GetSingletonAssetAtPath (string path, Type t) {
    path = path + "/" + t + ".asset";
    ScriptableObject asset = (ScriptableObject) AssetDatabase.LoadAssetAtPath(path, t);

    if (!asset) {
      asset = ScriptableObject.CreateInstance(t);
      AssetDatabase.CreateAsset(asset, path);
      AssetDatabase.SaveAssets();
    }

    return asset;
  }

  public static T GetSingletonAsset<T> () where T : ScriptableObject {
    string path = "Assets/bolt/resources/" + typeof(T).FullName + ".asset";
    T asset = (T) AssetDatabase.LoadAssetAtPath(path, typeof(T));

    if (!asset) {
      asset = ScriptableObject.CreateInstance<T>();
      AssetDatabase.CreateAsset(asset, path);
      AssetDatabase.SaveAssets();
    }

    return asset;
  }

  public static void SynchronizeWithController (BoltMecanimAsset asset) {
    AnimatorController ac = (AnimatorController) asset.controller;
    AnimatorControllerParameter[] parameters = GetParameters(ac);

    // verify we have a property for all parameters
    for (int i = 0; i < parameters.Length; ++i) {
      asset.properties = CreateProperty(asset.properties, parameters[i]);
    }

    // remove all outdated properties
    for (int i = 0; i < asset.properties.Length; ++i) {
      if (PropertyExists(parameters, asset.properties[i]) == false) {
        ArrayUtility.RemoveAt(ref asset.properties, i);

        // we need to check this index again so step back once
        i -= 1;
      }
    }
  }


  static bool PropertyExists (AnimatorControllerParameter[] parameters, BoltAssetProperty prop) {
    for (int i = 0; i < parameters.Length; ++i) {
      AnimatorControllerParameter param = parameters[i];

      if (param.name != prop.name) {
        continue;
      }

      if (param.type == AnimatorControllerParameterType.Float && prop.type == BoltAssetPropertyType.Float) {
        return true;
      }

      if (param.type == AnimatorControllerParameterType.Int && prop.type == BoltAssetPropertyType.Int) {
        return true;
      }

      if (param.type == AnimatorControllerParameterType.Bool && prop.type == BoltAssetPropertyType.Bool) {
        return true;
      }

      if (param.type == AnimatorControllerParameterType.Trigger && prop.type == BoltAssetPropertyType.Trigger) {
        return true;
      }
    }

    return false;
  }

  static BoltAssetProperty[] CreateProperty (BoltAssetProperty[] properties, AnimatorControllerParameter param) {
    for (int i = 0; i < properties.Length; ++i) {
      BoltAssetProperty prop = properties[i];

      if (prop.name != param.name) {
        continue;
      }

      if (prop.type == BoltAssetPropertyType.Float && param.type == AnimatorControllerParameterType.Float) {
        return properties;
      }

      if (prop.type == BoltAssetPropertyType.Int && param.type == AnimatorControllerParameterType.Int) {
        return properties;
      }

      if (prop.type == BoltAssetPropertyType.Bool && param.type == AnimatorControllerParameterType.Bool) {
        return properties;
      }

      if (prop.type == BoltAssetPropertyType.Trigger && param.type == AnimatorControllerParameterType.Trigger) {
        return properties;
      }
    }

    // create new property
    {
      BoltAssetProperty prop = new BoltAssetProperty();
      prop.name = param.name;

      switch (param.type) {
        case AnimatorControllerParameterType.Float: prop.type = BoltAssetPropertyType.Float; break;
        case AnimatorControllerParameterType.Int: prop.type = BoltAssetPropertyType.Int; break;
        case AnimatorControllerParameterType.Bool: prop.type = BoltAssetPropertyType.Bool; break;
        case AnimatorControllerParameterType.Trigger: prop.type = BoltAssetPropertyType.Trigger; break;
      }

      ArrayUtility.Add(ref properties, prop);
    }

    return properties;
  }

  static AnimatorControllerParameter[] GetParameters (AnimatorController ac) {
    AnimatorControllerParameter[] parameters = new AnimatorControllerParameter[ac.parameterCount];

    for (int i = 0; i < ac.parameterCount; ++i) {
      parameters[i] = ac.GetParameter(i);
    }

    return parameters;
  }

  public static void CreateAsset<T> (string name) where T : ScriptableObject {
    var asset = ScriptableObject.CreateInstance<T>();
    var path = AssetDatabase.GetAssetPath(Selection.activeObject);

    if (path == "") {
      path = "Assets";
    }
    else if (Path.GetExtension(path) != "") {
      path = path.Replace(Path.GetFileName(path), "");
    }

    AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(path + "/New" + name + ".asset"));
    AssetDatabase.SaveAssets();

    EditorUtility.FocusProjectWindow();

    Selection.activeObject = asset;
  }

  public static void InstallAsset (string file, Func<byte[]> data) {
    if (!File.Exists(file)) {
      InstallAsset(file, data());
    }
  }

  public static void InstallAsset (string file, byte[] data) {
    string path = Path.GetDirectoryName(file);

    try {
      Directory.CreateDirectory(path);
    } catch { }

    File.WriteAllBytes(file, data);
    AssetDatabase.ImportAsset(file, ImportAssetOptions.ForceUpdate);
  }

  public static List<IPAddress> FindEditorLanAddresses () {
    List<IPAddress> addresses = new List<IPAddress>();

    foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces()) {
      switch (nic.NetworkInterfaceType) {
        case NetworkInterfaceType.Ethernet:
        case NetworkInterfaceType.Ethernet3Megabit:
        case NetworkInterfaceType.FastEthernetFx:
        case NetworkInterfaceType.FastEthernetT:
        case NetworkInterfaceType.Wireless80211:
        case NetworkInterfaceType.GigabitEthernet:
          if (nic.OperationalStatus == OperationalStatus.Up) {
            IPInterfaceProperties p = nic.GetIPProperties();

            foreach (UnicastIPAddressInformation address in p.UnicastAddresses) {
              if (address.Address.AddressFamily == AddressFamily.InterNetwork) {
                addresses.Add(address.Address);
              }
            }
          }
          break;
      }
    }

    return addresses;
  }
}
