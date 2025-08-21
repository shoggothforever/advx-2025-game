using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Core;


    public class BaseBox : TimeReverse.ReversibleObject, TimeReverse.ITimeTrackable
    {
        public Rigidbody2D rb;
    public override void Start()
        {
            GameManager.Instance.addTrack(this);
            rb=GetComponent<Rigidbody2D>();
        }
        public TimeReverse.TimedAction RecordSnapshot()
        {
            return new TimeReverse.TimedAction
            {
                time = TimeManager.Instance.currentTime,
                objId = Id,
                pos = transform.position,
                velocity = rb.velocity,
                rotation = rb.rotation
                //payload = JsonUtility.ToJson(transform.position)
            };
        }
        public void ApplySnapshot(TimeReverse.TimedAction a)
        {
            //rb.MovePosition(a.pos);
            transform.position = Vector3.Lerp(transform.position, a.pos, 0.4f);
            if (ignoreV)
            {
                rb.velocity = Vector2.zero;
            }
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
        public bool canShrink=false;
        public bool ignoreV = true;
        private Transform originParent = null;



    public void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider.CompareTag("Player"))
            {
                originParent = collision.collider.transform.parent;
                collision.collider.transform.SetParent(transform, true);
            }
        }
        public void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.collider.CompareTag("Player"))
            {
                collision.collider.transform.SetParent(originParent);
            }
        }

    TimeReverse.ActionType  TimeReverse.ITimeTrackable.GetActionType()
    {
        return TimeReverse.ActionType.Position;     // 枚举：Position, AnimatorBool, AnimatorTrigger...
    }
    string TimeReverse.ITimeTrackable.Name()
    {
        return "base box";
    }
}