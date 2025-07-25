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
            public float time;          // ����ʱ globalTime
            public int objId;           // ����Ψһ id
            public ActionType type;     // ö�٣�Position, AnimatorBool, AnimatorTrigger...
            public Vector3 pos;
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