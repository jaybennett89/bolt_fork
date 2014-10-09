namespace Bolt {
  struct SceneLoadState {
    public const int STATE_LOADING = 1;
    public const int STATE_LOADING_DONE = 2;
    public const int STATE_CALLBACK_INVOKED = 3;

    public int State;
    public Scene Scene;

    public SceneLoadState BeginLoad(int index) {
      return new SceneLoadState { Scene = new Scene(index, this.Scene.Token + 1), State = SceneLoadState.STATE_LOADING };
    }

    public static SceneLoadState DefaultRemote() {
      return new SceneLoadState() { Scene = new Scene(-1, -1), State = SceneLoadState.STATE_CALLBACK_INVOKED };
    }

    public static SceneLoadState DefaultLocal() {
      return new SceneLoadState() { Scene = new Scene(-1, -1), State = SceneLoadState.STATE_LOADING_DONE };
    }
  }

  struct Scene {
    public readonly int Index;
    public readonly int Token;

    public Scene(int index, int token) {
      this.Index = index;
      this.Token = token;
    }

    public override int GetHashCode() {
      return Token ^ Index;
    }

    public override bool Equals(object obj) {
      return ((Scene)obj) == this;
    }

    public override string ToString() {
      return string.Format("[Scene Index={0} Token={1}]", Index, Token);
    }

    public static bool operator ==(Scene a, Scene b) {
      return a.Index == b.Index && a.Token == b.Token;
    }

    public static bool operator !=(Scene a, Scene b) {
      return a.Index != b.Index || a.Token != b.Token;
    }
  }
}