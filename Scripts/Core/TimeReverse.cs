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
            public Vector3 pos;
            public Vector3 velocity;
            public float rotation;
            public string payload;      // JSON �ַ������ɴ� Vector3��bool ��
            public bool activated;
        }
        // Ϊ���� JSON �����л� Vector3
        [System.Serializable] struct Vector3Snapshot { public float x, y, z; public Vector3 ToVector3() => new(x, y, z); }
        [System.Serializable] struct BoolSnapshot { public string name; public bool value; }
        public enum ActionType { Position, AnimatorBool, AnimatorTrigger, Destroy, Spawn,Manual }
        public interface ITimeTrackable
        {
         int Id { get; }
         TimedAction RecordSnapshot();   // ��ǰ֡�Ŀ���
         void ApplySnapshot(TimedAction a);
         ActionType GetActionType();
        }
        public abstract class ReversibleObject : MonoBehaviour
        {
            static int nextId =common.reversableItemInitialID ;
            public int Id { get; private set; }
            public void Awake()
            {
                Id = nextId++;
            }
            virtual public void Start() { }
        }
    }
}