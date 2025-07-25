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
            public float time;          // ����ʱ globalTime
            public int objId;           // ����Ψһ id
            public ActionType type;     // ö�٣�Position, AnimatorBool, AnimatorTrigger...
            public string payload;      // JSON �ַ������ɴ� Vector3��bool ��
        }
        // Ϊ���� JSON �����л� Vector3
        [System.Serializable] struct Vector3Snapshot { public float x, y, z; public Vector3 ToVector3() => new(x, y, z); }
        [System.Serializable] struct BoolSnapshot { public string name; public bool value; }
        public enum ActionType { Position, AnimatorBool, AnimatorTrigger, Destroy, Spawn }
        public interface ITimeTrackable
        {
         int Id { get; }
         TimedAction RecordSnapshot();   // ��ǰ֡�Ŀ���
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