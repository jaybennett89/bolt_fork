//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//public class LoadMapStateUpdater {
//  public static LoadMapState BeginLoad (LoadMapState state, string map, int token) {
//    Assert.True(token > state.token);
//    return new LoadMapState(map, token, LoadMapStage.Load);
//  }

//  public static LoadMapState FinishLoad (LoadMapState state, string map, int token) {
//    Assert.True(state.stage >= LoadMapStage.Load);
//    Assert.True(token >= state.token);

//    if (state.map == map && state.token == token) {
//      if (state.stage == LoadMapStage.Load) {
//        return state.ChangeStage(LoadMapStage.LoadDone);
//      } else {
//        return state;
//      }
//    } else {
//      return BeginLoad(state, map, token);
//    }
//  }

//  public static LoadMapState BeginCallback (LoadMapState state, string map, int token) {
//    Assert.True(state.stage >= LoadMapStage.LoadDone);
//    Assert.True(token >= state.token);

//    if (state.map == map && state.token == token) {
//      if (state.stage == LoadMapStage.LoadDone) {
//        return state.ChangeStage(LoadMapStage.Callback);
//      } else {
//        return state;
//      }
//    } else {
//      return BeginLoad(state, map, token);
//    }
//  }

//  public static LoadMapState FinishCallback (LoadMapState state, string map, int token) {
//    Assert.True(state.stage >= LoadMapStage.Callback);
//    Assert.True(token >= state.token);

//    if (state.map == map && state.token == token) {
//      if (state.stage == LoadMapStage.Callback) {
//        return state.ChangeStage(LoadMapStage.CallbackDone);
//      } else {
//        return state;
//      }
//    } else {
//      return BeginLoad(state, map, token);
//    }
//  }
//}

//struct LoadMapState {
//  public readonly int token;
//  public readonly string map;
//  public readonly LoadMapStage stage;

//  public LoadMapState (string map, int token, LoadMapStage stage) {
//    this.map = map;
//    this.token = token;
//    this.stage = stage;
//  }

//  public LoadMapState ChangeStage (LoadMapStage stage) {
//    return new LoadMapState(map, token, stage);
//  }

//  public override string ToString () {
//    return string.Format("[LoadMapState map={0} token={1} state={2}]", map, token, stage);
//  }
//}

//enum LoadMapStage {
//  Idle = 0,
//  Load = 1,
//  LoadDone = 2,
//  Callback = 3,
//  CallbackDone = 4
//}
