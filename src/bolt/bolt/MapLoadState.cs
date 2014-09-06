using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

struct Scene {
  public readonly int token;
  public readonly string name;

  public Scene (string map, int token) {
    this.name = map;
    this.token = token;
  }

  public override int GetHashCode () {
    return token ^ name.GetHashCode();
  }

  public override bool Equals (object obj) {
    return ((Scene) obj) == this;
  }

  public static bool operator == (Scene a, Scene b) {
    return a.name == b.name && a.token == b.token;
  }

  public static bool operator != (Scene a, Scene b) {
    return a.name != b.name || a.token != b.token;
  }
}

struct SceneLoadState {
  public readonly Scene scene;
  public readonly SceneLoadStage stage;

  SceneLoadState (Scene map, SceneLoadStage stage) {
    this.scene = map;
    this.stage = stage;
  }

  SceneLoadState ChangeStage (SceneLoadStage stage) {
    return new SceneLoadState(this.scene, stage);
  }

  public SceneLoadState BeginLoad (Scene scene) {
    if (this.scene != scene) {
      Assert.True(scene.token > this.scene.token);
      return new SceneLoadState(scene, SceneLoadStage.Load);
    }

    return this;
  }

  public SceneLoadState FinishLoad (Scene finished, Scene target) {
    Assert.True(this.stage >= SceneLoadStage.Load);

    if (target == finished) {
      Assert.True(scene == target);

      if (this.stage == SceneLoadStage.Load) {
        return ChangeStage(SceneLoadStage.LoadDone);
      }

      return this;
    } else {
      return BeginLoad(target);
    }
  }

  public SceneLoadState BeginCallback (SceneLoadState localState) {
    if (this.scene == localState.scene) {
      if (this.stage == SceneLoadStage.LoadDone && localState.stage >= SceneLoadStage.LoadDone) {
        return ChangeStage(SceneLoadStage.Callback);
      }

      return this;
    } else {
      return BeginLoad(localState.scene);
    }
  }

  public SceneLoadState FinishCallback (Scene scene) {
    Assert.True(this.stage >= SceneLoadStage.Callback);
    Assert.True(scene.token >= this.scene.token);

    if (this.scene == scene) {
      if (this.stage == SceneLoadStage.Callback) {
        return ChangeStage(SceneLoadStage.CallbackDone);
      } else {
        return this;
      }
    } else {
      return BeginLoad(scene);
    }
  }

  public override string ToString () {
    return string.Format("[LoadMapState scene={0} token={1} stage={2}]", scene.name, scene.token, stage);
  }
}

enum SceneLoadStage {
  Idle = 0,
  Load = 1,
  LoadDone = 2,
  Callback = 3,
  CallbackDone = 4
}
