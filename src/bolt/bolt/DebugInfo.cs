using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt {
  public class DebugInfo : MonoBehaviour {
    static GUIStyle labelStyle;
    static GUIStyle labelStyleBold;
    static Texture2D boltIconTexture;
    static Texture2D backgroundTexture;
    static internal bool showEntityDebugInfo = true;
    static internal HashSet<InstanceId> ignoreList = new HashSet<InstanceId>();

    public static int PollTime { get; internal set; }
    public static int SendTime { get; internal set; }
    public static bool Enabled { get; private set; }

    public static void Ignore(BoltEntity entity) {
      ignoreList.Add(entity.Entity.InstanceId);
    }

    public static void DrawBackground(Rect r) {
      GUI.color = new Color(0, 0, 0, 0.75f);
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
          backgroundTexture.SetPixels(
              new Color[] {
                    Color.white,
                    Color.white,
                    Color.white,
                    Color.white,
                }
          );
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
      if (FindObjectOfType<DebugInfo>()) {
        return;
      }

      GameObject go;

      go = new GameObject("BoltDebugInfo");
      go.AddComponent<DebugInfo>();

      GameObject.DontDestroyOnLoad(go);

      Enabled = true;
    }

    public static void Hide() {
      DebugInfo found = FindObjectOfType<DebugInfo>();

      if (found) {
        GameObject.Destroy(found.gameObject);
      }

      Enabled = false;
    }

    Vector2 debugInfoScroll;

    Color GetColor(int current, int bad) {
      return GetColor(Mathf.Clamp01((float)current / (float)bad));
    }

    Color GetColor(float t) {
      return Color.Lerp(BoltGUI.Green, BoltGUI.Error, t);
    }

    void OnGUI() {
      BoltNetworkInternal.DebugDrawer.IsEditor(false);

      {
        Rect r = new Rect(10, Screen.height - 30, 300, 20);

        DrawBackground(r);

        r.xMin += 5;
        r.yMin += 5;

        GUILayout.BeginArea(r);
        GUILayout.BeginHorizontal();

        GUILayout.Label("Bolt Performance: ", LabelStyleBold);

        string pollTime = PollTime.ToString().PadLeft(3, '0');
        string sendTime = SendTime.ToString().PadLeft(3, '0');

        GUILayout.Label(string.Format("Poll {0} ms", pollTime), LabelStyleColor(GetColor(PollTime, 16)));
        GUILayout.Label(string.Format("Send {0} ms", sendTime), LabelStyleColor(GetColor(SendTime, 16)));
        GUILayout.Label(string.Format("Total Entities {0}", BoltCore._entities.count), LabelStyle);

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
      }
      {
        Camera c = Camera.main;

        if (!c) {
          return;
        }

        Entity entity = BoltCore._entities
          .Where(x => ignoreList.Contains(x.InstanceId) == false)
          .Where(x => {
              Vector3 vp = c.WorldToViewportPoint(x.UnityObject.transform.position);
              return vp.z >= 0 && vp.x >= 0 && vp.x <= 1 && vp.y >= 0 && vp.y <= 1;
          })
          .OrderBy(x => { 
            Vector3 vc = new Vector3(0.5f, 0.5f, 0f);
            Vector3 vp = c.WorldToViewportPoint(x.UnityObject.transform.position);
            vp.z = 0;

            return (vc - vp).sqrMagnitude;
          })
          .FirstOrDefault();

        if (entity) {

          Rect r = new Rect(Screen.width - 410, 10, 400, Screen.height - 20);

          DrawBackground(r);

          r.xMin += 10;
          r.xMax -= 10;

          r.yMin += 10;
          r.yMax -= 10;

          GUILayout.BeginArea(r);

          debugInfoScroll = GUILayout.BeginScrollView(debugInfoScroll, false, false, GUIStyle.none, GUIStyle.none);
          GUILayout.BeginVertical();

          var state = (State)entity.Serializer;

          LabelBold("Entity Info");
          LabelField("Name", entity.UnityObject.gameObject.name);
          LabelField("UniqueId", entity.UniqueId);
          LabelField("World Position", entity.UnityObject.transform.position);
          LabelField("Frame Count", state.Frames.count);
          LabelField("Frame Latest Number", state.Frames.last.Number);
          LabelField("Frame Server Number", BoltNetwork.serverFrame);

          LabelBold("Proxy Data");

          foreach (BoltConnection connection in BoltCore._connections) {
            int skipCount = connection._entityChannel.GetSkippedUpdates(entity);
            if (skipCount >= 0) {
              LabelField(connection.remoteEndPoint, "Skip: " + skipCount + " / Priority: " + connection._entityChannel.GetPriority(entity));
            }
          }

          entity.Serializer.DebugInfo();

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
  }
}
