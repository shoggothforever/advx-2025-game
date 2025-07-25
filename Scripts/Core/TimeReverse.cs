using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Utils;
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
            public Vector3 pos;
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
            void SetKe();
        bool DetectMove();
        }
        [RequireComponent(typeof(Rigidbody2D))]
        public class ReversibleObject : MonoBehaviour
        {
            static int nextId =common.reversableItemInitialID ;
            public int Id { get; private set; }
            public Rigidbody2D rb;
            public Vector2 lastVelocity;
            public void Awake()
            {
                Id = nextId++;
                rb = GetComponent<Rigidbody2D>();
            }
            
        }
    }
}