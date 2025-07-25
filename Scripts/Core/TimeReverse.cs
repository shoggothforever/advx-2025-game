using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SaveYourself.Core
{
    public class TimeReverse : MonoBehaviour
    {
        [System.Serializable]
        public struct TimedAction
        {
            public float time;          // 发生时 globalTime
            public int objId;           // 物体唯一 id
            public ActionType type;     // 枚举：Position, AnimatorBool, AnimatorTrigger...
            public string payload;      // JSON 字符串，可存 Vector3、bool 等
        }
        // 为了让 JSON 能序列化 Vector3
        [System.Serializable] struct Vector3Snapshot { public float x, y, z; public Vector3 ToVector3() => new(x, y, z); }
        [System.Serializable] struct BoolSnapshot { public string name; public bool value; }
        public enum ActionType { Position, AnimatorBool, AnimatorTrigger, Destroy, Spawn }
        public interface ITimeTrackable
        {
         int Id { get; }
         TimedAction RecordSnapshot();   // 当前帧拍快照
         void ApplySnapshot(TimedAction a);
        }
        [RequireComponent(typeof(Rigidbody2D))]
        public class ReversibleObject : MonoBehaviour, ITimeTrackable
        {
            static int nextId = 100;
            Rigidbody2D rb;
            Animator anim;

            public int Id { get; private set; }

            void Awake()
            {
                Id = nextId++;
                rb = GetComponent<Rigidbody2D>();
                anim = GetComponent<Animator>();
                //TimeManager.Instance.Register(this);
            }

            public TimedAction RecordSnapshot()
            {
                var p = new Vector3Snapshot();
                p.x = transform.position.x;
                p.y = transform.position.y;
                p.z = transform.position.z;
                return new TimedAction
                {
                    time = TimeManager.Instance.currentTime,
                    objId = Id,
                    type = ActionType.Position,
                    payload = JsonUtility.ToJson(p)
                };
            }

            public void ApplySnapshot(TimedAction a)
            {
                switch (a.type)
                {
                    case ActionType.Position:
                        var p = JsonUtility.FromJson<Vector3Snapshot>(a.payload);
                        transform.position = p.ToVector3();
                        rb.velocity = Vector2.zero;
                        break;
                    case ActionType.AnimatorBool:
                        var b = JsonUtility.FromJson<BoolSnapshot>(a.payload);
                        anim.SetBool(b.name, b.value);
                        break;
                }
            }
        }
    }
}