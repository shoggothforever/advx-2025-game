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
            //rb.MovePosition(a.pos);
            transform.position = a.pos;
            if (ignoreV)
            {
                rb.velocity = Vector2.zero;
            }
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
        public void SetKe()
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        public void Enlarge()
        {
            // 1. 视觉
            transform.localScale = Vector3.one * 2f;
            // 2. 碰撞
            var bc = GetComponent<BoxCollider2D>();
            if (bc)
            {
                bc.size = bc.size * 2f;        // 或者 bc.size = new Vector2(2,2);
                bc.offset = bc.offset * 2f;    // 如果之前有偏移
            }
        }
        public bool canPushToLeft;
        public bool canPushToRight;
        public bool canPressDown;
        public bool canShrink=false;
        public bool ignoreV = true;
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