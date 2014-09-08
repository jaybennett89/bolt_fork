using System;
using System.Collections.Generic;
using System.Reflection;
using UdpKit;
using UnityEngine;

static class BoltRuntimeReflection {
  static public UdpPlatform InvokeCreatePlatformMethod () {
    BindingFlags flags = BindingFlags.Public | BindingFlags.Static;

    foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
      if (asm.GetName().Name == "Assembly-CSharp") {
        return (UdpPlatform) asm.GetType("BoltNetworkUtils").GetMethod("CreateUdpPlatform", flags).Invoke(null, new object[0]);
      }
    }

    throw new BoltException("Could not find type 'BoltNetworkUtils'");
  }

  static public byte[] GetUserAssemblyHash () {
    BindingFlags flags = BindingFlags.Public | BindingFlags.Static;

    foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
      if (asm.GetName().Name == "Assembly-CSharp") {
        return (byte[]) asm.GetType("BoltUserAssemblyHash").GetField("value", flags).GetValue(null);
      }
    }

    throw new BoltException("Could not find type 'BoltUserAssemblyHash'");
  }

  static public List<STuple<BoltGlobalBehaviourAttribute, Type>> FindGlobalObjectTypes () {
    List<STuple<BoltGlobalBehaviourAttribute, Type>> result = new List<STuple<BoltGlobalBehaviourAttribute, Type>>();

    foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
        try
        {
            foreach (Type type in asm.GetTypes())
            {
                if (typeof(MonoBehaviour).IsAssignableFrom(type))
                {
                    var attrs = (BoltGlobalBehaviourAttribute[])type.GetCustomAttributes(typeof(BoltGlobalBehaviourAttribute), false);
                    if (attrs.Length == 1)
                    {
                        result.Add(new STuple<BoltGlobalBehaviourAttribute, Type>(attrs[0], type));
                    }
                }
            }
        }
        catch
        {
            // just eat this, a bit dangerous but meh.
        }
    }

    return result;
  }
}
