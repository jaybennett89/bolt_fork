using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

struct Map {
  public readonly int token;
  public readonly string name;

  public Map (string map, int token) {
    this.name = map;
    this.token = token;
  }

  public override int GetHashCode () {
    return token ^ name.GetHashCode();
  }

  public override bool Equals (object obj) {
    return ((Map) obj) == this;
  }

  public static bool operator == (Map a, Map b) {
    return a.name == b.name && a.token == b.token;
  }

  public static bool operator != (Map a, Map b) {
    return a.name != b.name || a.token != b.token;
  }
}

struct MapLoadState {
  public readonly Map map;
  public readonly MapLoadStage stage;

  MapLoadState (Map map, MapLoadStage stage) {
    this.map = map;
    this.stage = stage;
  }

  MapLoadState ChangeStage (MapLoadStage stage) {
    return new MapLoadState(this.map, stage);
  }

  public MapLoadState BeginLoad (Map map) {
    if (this.map != map) {
      Assert.True(map.token > this.map.token);
      return new MapLoadState(map, MapLoadStage.Load);
    }

    return this;
  }

  public MapLoadState FinishLoad (Map finished, Map target) {
    Assert.True(this.stage >= MapLoadStage.Load);

    if (target == finished) {
      Assert.True(map == target);

      if (this.stage == MapLoadStage.Load) {
        return ChangeStage(MapLoadStage.LoadDone);
      }

      return this;
    } else {
      return BeginLoad(target);
    }
  }

  public MapLoadState BeginCallback (MapLoadState localState) {
    if (this.map == localState.map) {
      if (this.stage == MapLoadStage.LoadDone && localState.stage >= MapLoadStage.LoadDone) {
        return ChangeStage(MapLoadStage.Callback);
      }

      return this;
    } else {
      return BeginLoad(localState.map);
    }
  }

  public MapLoadState FinishCallback (Map map) {
    Assert.True(this.stage >= MapLoadStage.Callback);
    Assert.True(map.token >= this.map.token);

    if (this.map == map) {
      if (this.stage == MapLoadStage.Callback) {
        return ChangeStage(MapLoadStage.CallbackDone);
      } else {
        return this;
      }
    } else {
      return BeginLoad(map);
    }
  }

  public override string ToString () {
    return string.Format("[LoadMapState map={0} token={1} state={2}]", map.name, map.token, stage);
  }
}

enum MapLoadStage {
  Idle = 0,
  Load = 1,
  LoadDone = 2,
  Callback = 3,
  CallbackDone = 4
}
