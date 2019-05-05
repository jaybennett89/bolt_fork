#pragma warning disable 1591

using System;
using UnityEngine;

[Serializable]
public class BoltAssetProperty {
  [Serializable]
  public class IntSettings {
    public int byteBits = 8;
    public int shortBits = 16;
    public int intBits = 32;
    public int longBits = 64;
  }

  [Serializable]
  public class FloatSettings {
    public bool interpolate = false;
    public BoltAssetFloatCompression compression;
  }

  [Serializable]
  public class VectorSettings {
    public bool interpolate = false;
    public BoltAssetFloatCompression compression;
    public BoltAssetAxes axes = BoltAssetAxes.X | BoltAssetAxes.Y | BoltAssetAxes.Z | BoltAssetAxes.W;
  }

  [Serializable]
  public class QuaternionSettings {
    public bool interpolate = false;
    public BoltAssetFloatCompression compression = BoltAssetFloatCompression.None;
    public BoltAssetAxes axes = BoltAssetAxes.X | BoltAssetAxes.Y | BoltAssetAxes.Z;
  }

  [Serializable]
  public class StringSettings {
    public int maxLength = -1;
    public BoltAssetStringEncoding encoding = BoltAssetStringEncoding.UTF8;
  }

  [Serializable]
  public class MecanimSettings {
    public BoltMecanimAsset mecanimAsset;
  }

  [Serializable]
  public class TransformSettings {
    public BoltAssetTransformModes mode = BoltAssetTransformModes.InterpolatedSnapshots;
    public int maxForwardExtrapolation = 2;
    public int maxInterpTime = 1;

    public BoltAssetAxes posAxes = BoltAssetAxes.X | BoltAssetAxes.Y | BoltAssetAxes.Z;
    public BoltAssetFloatCompression posCompression = BoltAssetFloatCompression.None;

    public BoltAssetAxes rotAxes = BoltAssetAxes.X | BoltAssetAxes.Y | BoltAssetAxes.Z;
    public BoltAssetFloatCompression rotCompression = BoltAssetFloatCompression.None;

    public float velZeroTolerance = 0.01f;
    public BoltAssetTransformVelocityMode velMode = BoltAssetTransformVelocityMode.SendFromOwner;
    public BoltAssetFloatCompression velCompression = BoltAssetFloatCompression.None;
    public BoltAssetAxes velAxes = BoltAssetAxes.X | BoltAssetAxes.Y | BoltAssetAxes.Z;

    public float accZeroTolerance = 0.01f;
    public BoltAssetTransformAccelerationMode accMode = BoltAssetTransformAccelerationMode.SendFromOwner;
    public BoltAssetFloatCompression accCompression = BoltAssetFloatCompression.None;

    public bool inferVelocity {
      get { return false; }
      //get { return velMode == BoltAssetTransformVelocityMode.InferFromPosition; }
    }

    public bool inferAcceleration {
      get { return accMode == BoltAssetTransformAccelerationMode.InferFromVelocity; }
    }

    public bool useAcceleration {
      get { return accMode != BoltAssetTransformAccelerationMode.DontUse; }
    }

    public bool rotAllAxes {
      get { return rotAxes == (BoltAssetAxes.X | BoltAssetAxes.Y | BoltAssetAxes.Z); }
    }
  }

  [Serializable]
  public class AssetSettingsMecanim {
    public float interpolationTime = 0f;
  }

  [Serializable]
  public class AssetSettingsCommand {
    public bool synchronize = true;
  }

  [NonSerialized]
  public int bit;

  [NonSerialized]
  public bool delete;

  [NonSerialized]
  public int shift;

  public string name;
  public bool enabled = true;
  public bool foldout = true;
  public BoltAssetSyncMode syncMode = BoltAssetSyncMode.Changed;
  public BoltAssetSyncTarget syncTarget = BoltAssetSyncTarget.Proxy | BoltAssetSyncTarget.Controller;
  public BoltAssetPropertyType type;
  public BoltAssetPropertyOptions options;
  public IntSettings intSettings = new IntSettings();
  public FloatSettings floatSettings = new FloatSettings();
  public VectorSettings vectorSettings = new VectorSettings();
  public QuaternionSettings quaternionSettings = new QuaternionSettings();
  public StringSettings stringSettings = new StringSettings();
  public MecanimSettings mecanimSettings = new MecanimSettings();
  public TransformSettings transformSettings = new TransformSettings();
  public AssetSettingsMecanim assetSettingsMecanim = new AssetSettingsMecanim();
  public AssetSettingsCommand assetSettingsCommand = new AssetSettingsCommand();

  public bool nameIsCSharpId {
    get { return true; }
  }

  public string backingFieldName {
    get { return "__" + name + "__"; }
  }

  public string runtimeType {
    get {
      if ((type == BoltAssetPropertyType.Mecanim) && mecanimSettings.mecanimAsset) {
        return mecanimSettings.mecanimAsset.interfaceName;
      }

      if (type == BoltAssetPropertyType.Trigger) {
        return "ulong";
      }

      return type.ToString();
    }
  }

  public bool requiresConnection {
    get { return type == BoltAssetPropertyType.Entity; }
  }

  public bool hasNotifyCallback {
    get {
      return isReference == false && smoothed == false && ((options & BoltAssetPropertyOptions.Notify) == BoltAssetPropertyOptions.Notify);
    }
  }

  public bool isDefault {
    get {
      switch (type) {
        case BoltAssetPropertyType.Transform:
        case BoltAssetPropertyType.Mecanim:
          return true;
      }

      return false;
    }
  }

  public bool isReference {
    get {
      switch (type) {
        case BoltAssetPropertyType.Transform:
        case BoltAssetPropertyType.Mecanim:
        case BoltAssetPropertyType.Custom:
          return true;
      }

      return false;
    }
  }

  public bool smoothed {
    get {
      switch (type) {
        case BoltAssetPropertyType.Float:
          return floatSettings.interpolate;

        case BoltAssetPropertyType.Vector2:
        case BoltAssetPropertyType.Vector3:
        case BoltAssetPropertyType.Vector4:
          return vectorSettings.interpolate;

        case BoltAssetPropertyType.Quaternion:
          return quaternionSettings.interpolate;
      }

      return false;
    }
  }
}

#pragma warning restore 1591