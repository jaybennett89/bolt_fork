using System;

public struct BoltApplicationVersion {
  public readonly Guid guid;
  public readonly int major;
  public readonly int minor;
  public readonly int patch;
  public readonly int build;

  public string guidString {
    get {
      return guid.ToString();
    }
  }

  public string versionString {
    get {
      return string.Format("{0}.{1}.{2}.{3}", major, minor, patch, build);
    }
  }

  public BoltApplicationVersion (Guid guid, int major) {
    this.guid = guid;
    this.major = major;
    this.minor = 0;
    this.patch = 0;
    this.build = 0;
  }

  public BoltApplicationVersion (Guid guid, int major, int minor) {
    this.guid = guid;
    this.major = major;
    this.minor = minor;
    this.patch = 0;
    this.build = 0;
  }

  public BoltApplicationVersion (Guid guid, int major, int minor, int patch) {
    this.guid = guid;
    this.major = major;
    this.minor = minor;
    this.patch = patch;
    this.build = 0;
  }

  public BoltApplicationVersion (Guid guid, int major, int minor, int patch, int build) {
    this.guid = guid;
    this.major = major;
    this.minor = minor;
    this.patch = patch;
    this.build = build;
  }

  public override string ToString () {
    return string.Format("[App Guid={0} Version={1}]", guid, versionString);
  }
}
