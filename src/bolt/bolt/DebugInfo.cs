using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt {
  [Documentation(Ignore = true)]
  public class DebugInfo : MonoBehaviour {
    Vector2 debugInfoScroll;

    static Entity locked;
    static GUIStyle labelStyle;
    static GUIStyle labelStyleBold;
    static Texture2D boltIconTexture;
    static Texture2D backgroundTexture;
    static internal bool showEntityDebugInfo = true;
    static internal HashSet<NetworkId> ignoreList = new HashSet<NetworkId>();

    public static int PollTime { get; internal set; }
    public static int SendTime { get; internal set; }
    public static bool Enabled { get; private set; }

    public static void Ignore(BoltEntity entity) {
      ignoreList.Add(entity.Entity.NetworkId);
    }

    public static void DrawBackground(Rect r) {
      GUI.color = new Color(0, 0, 0, 0.85f);
      GUI.DrawTexture(r, BackgroundTexture);
      GUI.color = Color.white;
    }

    public static Texture2D BoltIconTexture {
      get {
        if (!boltIconTexture) {
          boltIconTexture = (Texture2D)Resources.Load("BoltIcon", typeof(Texture2D));
        }

        return boltIconTexture;
      }
    }

    public static Texture2D BackgroundTexture {
      get {
        if (!backgroundTexture) {
          backgroundTexture = new Texture2D(2, 2);
          backgroundTexture.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white, });
        }

        return backgroundTexture;
      }
    }

    public static GUIStyle LabelStyle {
      get {
        if (labelStyle == null) {
          labelStyle = new GUIStyle();
          labelStyle.normal.textColor = Color.white;
          labelStyle.fontStyle = FontStyle.Normal;
          labelStyle.fontSize = 10;
          labelStyle.alignment = TextAnchor.UpperLeft;
          labelStyle.clipping = TextClipping.Clip;
        }

        return labelStyle;
      }
    }

    public static GUIStyle LabelStyleBold {
      get {
        if (labelStyleBold == null) {
          labelStyleBold = new GUIStyle(LabelStyle);
          labelStyleBold.fontStyle = FontStyle.Bold;
        }

        return labelStyleBold;
      }
    }


    public static GUIStyle LabelStyleColor(Color color) {
      GUIStyle style;

      style = new GUIStyle(LabelStyle);
      style.normal.textColor = color;

      return style;
    }

    public static void Label(object value) {
      GUILayout.Label(value.ToString(), LabelStyle, GUILayout.Height(11));
    }

    public static void LabelBold(object value) {
      GUILayout.Label(value.ToString(), LabelStyleBold, GUILayout.Height(11));
    }

    public static void LabelField(object label, object value) {
      GUILayout.BeginHorizontal();
      GUILayout.Label(label.ToString(), LabelStyle, GUILayout.Height(11), GUILayout.Width(175));
      GUILayout.Label(value.ToString(), LabelStyle, GUILayout.Height(11));
      GUILayout.EndHorizontal();
    }

    public static void Show() {
      if (FindObjectOfType(typeof(DebugInfo))) {
        return;
      }

      GameObject go;

      go = new GameObject("BoltDebugInfo");
      go.AddComponent<DebugInfo>();

      GameObject.DontDestroyOnLoad(go);

      Enabled = true;
    }

    public static void Hide() {
      DebugInfo found = FindObjectOfType(typeof(DebugInfo)) as DebugInfo;

      if (found) {
        GameObject.Destroy(found.gameObject);
      }

      Enabled = false;
    }

    Color GetColor(int current, int bad) {
      return GetColor(Mathf.Clamp01((float)current / (float)bad));
    }

    Color GetColor(float t) {
      return Color.Lerp(BoltGUI.Debug, BoltGUI.Error, t);
    }

    void DrawEntity(BoltEntity entity) {
      if (entity && entity.isAttached) {
        Camera c = Camera.main;

        if (c) {
          Vector3 vp = c.WorldToViewportPoint(entity.transform.position);

          if (vp.z >= 0 && vp.x >= 0 && vp.x <= 1 && vp.y >= 0 && vp.y <= 1) {
            Vector3 sp = c.WorldToScreenPoint(entity.transform.position);
            Rect r = new Rect(sp.x - 8, (Screen.height - sp.y) - 8, 16, 16);
            DebugInfo.DrawBackground(r);

            GUI.DrawTexture(r, DebugInfo.BoltIconTexture);
          }
        }
      }
    }

    void OnGUI() {
      BoltNetworkInternal.DebugDrawer.IsEditor(false);

      {
        Rect r = new Rect(10, Screen.height - 30, Screen.width - 20, 20);

        DrawBackground(r);

        r.xMin += 5;
        r.yMin += 5;

        GUILayout.BeginArea(r);
        GUILayout.BeginHorizontal();

        GUILayout.Label("Bolt Performance: ", LabelStyleBold);

        string pollTime = PollTime.ToString().PadLeft(3, '0');
        string sendTime = SendTime.ToString().PadLeft(3, '0');

        //GUILayout.Label("Send: " + System.Math.Round(BoltCore.SendTime.TotalMilliseconds, 2));
        //GUILayout.Label("Auto: " + System.Math.Round(BoltCore.AutoscopeTime.TotalMilliseconds, 2));

        //GUILayout.Label("Poll: " + System.Math.Round(BoltCore.PollNetworkTime.TotalMilliseconds, 2));
        //GUILayout.Label("Callbacks: " + System.Math.Round(BoltCore.InvokeRemoteSceneCallbacksTime.TotalMilliseconds, 2));
        //GUILayout.Label("StepLocal: " + System.Math.Round(BoltCore.SimulateLocalAndControlledEntitiesTime.TotalMilliseconds, 2));
        //GUILayout.Label("Step: " + System.Math.Round(BoltCore.StepNonControlledRemoteEntitiesTime.TotalMilliseconds, 2));
        //GUILayout.Label("Adjust: " + System.Math.Round(BoltCore.AdjustEstimatedRemoteFramesTime.TotalMilliseconds, 2));

        GUILayout.Label(string.Format("Poll {0} ms", pollTime), LabelStyleColor(GetColor(PollTime, 16)));
        GUILayout.Label(string.Format("Send {0} ms", sendTime), LabelStyleColor(GetColor(SendTime, 16)));
        GUILayout.Label(string.Format("Active Entities {0}", BoltCore._entities.Count(x => !x.IsFrozen)), LabelStyle);
        GUILayout.Label(string.Format("Frozen Entities {0}", BoltCore._entities.Count(x => x.IsFrozen)), LabelStyle);

        //
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
      }
      {
        Camera c = Camera.main;

        if (!c) {
          return;
        }

        //Vector3 mp;
        //mp = c.ScreenToViewportPoint(Input.mousePosition);
        //mp.z = 0;

        foreach (Entity en in BoltCore._entitiesOK) {
          DrawEntity(en.UnityObject);
        }

        //Entity hover = BoltCore._entitiesOK
        //  .Where(x => ignoreList.Contains(x.NetworkId) == false)
        //  .Where(x => c.WorldToViewportPoint(x.UnityObject.transform.position).ViewPointIsOnScreen())

        //  //.Where(x => {
        //  //  Vector3 m;
        //  //  m = Input.mousePosition;
        //  //  m.z = 0;
        //  //  Vector3 p;
        //  //  p = c.WorldToScreenPoint(x.UnityObject.transform.position);
        //  //  p.z = 0;
        //  //  return (m - p).sqrMagnitude < (32 * 32);
        //  //})

        //  .OrderBy(x => {
        //    Vector3 center = new Vector3(0.5f, 0.5f, 0f);

        //    Vector3 vp;
        //    vp = c.WorldToViewportPoint(x.UnityObject.transform.position);
        //    vp.z = 0;

        //    return (center - vp).sqrMagnitude;
        //  })
        //  .FirstOrDefault();

        //if (Input.GetKeyDown(KeyCode.Home)) {
        //  locked = hover;
        //}

        //Entity entity = locked ? locked : hover;

        if (Input.GetKeyDown(KeyCode.Home)) {
          locked = BoltCore._entities
            .Where(x => ignoreList.Contains(x.NetworkId) == false)
            .Where(x => c.WorldToViewportPoint(x.UnityObject.transform.position).ViewPointIsOnScreen())
            .OrderBy(x => {
              Vector3 center = new Vector3(0.5f, 0.5f, 0f);

              Vector3 vp;
              vp = c.WorldToViewportPoint(x.UnityObject.transform.position);
              vp.z = 0;

              return (center - vp).sqrMagnitude;
            })
            .FirstOrDefault();
        }
        
        if (locked) {
          Rect r = new Rect(Screen.width - 410, 10, 400, Screen.height - 20);

          DrawBackground(r);

          r.xMin += 10;
          r.xMax -= 10;

          r.yMin += 10;
          r.yMax -= 10;

          GUILayout.BeginArea(r);

          debugInfoScroll = GUILayout.BeginScrollView(debugInfoScroll, false, false, GUIStyle.none, GUIStyle.none);
          GUILayout.BeginVertical();

          var state = (NetworkState)locked.Serializer;

          if (Input.GetKeyDown(KeyCode.L)) {
            BoltNetworkInternal.DebugDrawer.SelectGameObject(locked.UnityObject.gameObject);
          }

          LabelBold("Entity Info");
          LabelField("Name", locked.UnityObject.gameObject.name);
          LabelField("Network Id", locked.NetworkId);
          LabelField("Is Frozen", locked.IsFrozen);
          LabelField("Animator", state.Animator == null ? "NULL" : state.Animator.gameObject.name);
          LabelField("Entity Parent", locked.HasParent ? locked.Parent.UnityObject.ToString() : "NULL");

          //LabelField("Transform Parent", entity.UnityObject.transform.parent == null ? "NULL" : entity.UnityObject.transform.parent.GetComponent<BoltEntity>().ToString());

          LabelField("Has Control", locked.HasControl);

          if (state.Animator != null) {
            for (int i = 0; i < state.Animator.layerCount; ++i) {
              LabelField("  Layer", state.Animator.GetLayerName(i));

#if UNITY5
              var clips = state.Animator.GetCurrentAnimatorClipInfo(i);
#else
              var clips = state.Animator.GetCurrentAnimationClipState(i);
#endif

              foreach (var clip in clips) {
                LabelField("    Clip", string.Format("{0} (weight: {1})", clip.clip.name, clip.weight));
              }
            }
          }

          if (locked.IsOwner) {
            LabelBold("");
            LabelBold("Connection Priorities");

            foreach (BoltConnection cn in BoltNetwork.connections) {
              LabelField("Connection#" + cn.udpConnection.ConnectionId, cn._entityChannel.GetPriority(locked).ToString());
            }
          }

          if (locked.IsOwner == false) {
            LabelBold("");
            LabelBold("Frame Info");
            LabelField("Buffer Count", state.Frames.count);
            LabelField("Latest Received Number", state.Frames.last.Frame);
            LabelField("Diff (Should be < 0)", BoltNetwork.serverFrame - state.Frames.last.Frame);
          }

          LabelBold("");
          LabelBold("World Info");
          LabelField("Position", locked.UnityObject.transform.position);
          LabelField("Distance From Camera", (c.transform.position - locked.UnityObject.transform.position).magnitude);

          locked.Serializer.DebugInfo();

          GUILayout.EndVertical();
          GUILayout.EndScrollView();
          GUILayout.EndArea();
        }

        if (Input.GetKey(KeyCode.PageUp)) {
          debugInfoScroll.y = Mathf.Max(debugInfoScroll.y - 10, 0);
        }

        if (Input.GetKey(KeyCode.PageDown)) {
          debugInfoScroll.y = Mathf.Min(debugInfoScroll.y + 10, 2000);
        }
      }
    }

    internal static void SetupAndShow() {
      if (BoltRuntimeSettings.instance.showDebugInfo) {
        ignoreList = new HashSet<NetworkId>();

        // show us!
        Show();
      }
    }
  }
}
