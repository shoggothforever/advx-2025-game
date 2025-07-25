using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Core;

namespace SaveYourself.Interact
{
    public class BaseBox : TimeReverse.ReversibleObject, TimeReverse.ITimeTrackable
    {
        public void  ApplySnapshot(TimeReverse.TimedAction a)
        {
            //var p = JsonUtility.FromJson<Vector3>(a.payload);
            //rb.MovePosition(a.pos);
            transform.position = a.pos;
            rb.velocity = Vector2.zero;
        }

        public TimeReverse.TimedAction RecordSnapshot()
        {
            return new TimeReverse.TimedAction
            {
                time = TimeManager.Instance.currentTime,
                objId = Id,
                type = TimeReverse.ActionType.Position,
                pos = transform.position,
                //payload = JsonUtility.ToJson(transform.position)
            };
        }
        public bool DetectMove()
        {
            bool wasMoving = lastVelocity.sqrMagnitude > 0.02f;
            bool isMoving = rb.velocity.sqrMagnitude > 0.02f;
            return wasMoving != isMoving;
        }
        void Update()
        {
            if(TimeManager.Instance.phase == TimeManager.Phase.Reverse)
            {
                lastVelocity = rb.velocity;
            }

        }
        public bool canPushToLeft;
        public bool canPushToRight;
        public bool canPressDown;
        [SerializeField] LayerMask stoneLayer = 0;
        [SerializeField] float raycastOffsetX = 0;
        [SerializeField] float detectDis = 1f;
        bool isReachTargetPos;
        bool CanPushToLeft()
        {
            return false;
        }
        bool CanPushToRight()
        {
            return false;
        } 
        bool CanPressDown()
        {
            return false;
        }


    }
}