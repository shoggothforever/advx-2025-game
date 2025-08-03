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
            transform.position = Vector3.Lerp(transform.position, a.pos, 0.4f);
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
                velocity = rb.velocity,
                rotation = rb.rotation
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
            // 1. �Ӿ�
            transform.localScale = Vector3.one * 2f;
            // 2. ��ײ
            var bc = GetComponent<BoxCollider2D>();
            if (bc)
            {
                bc.size = bc.size * 2f;        // ���� bc.size = new Vector2(2,2);
                bc.offset = bc.offset * 2f;    // ���֮ǰ��ƫ��
            }
        }
        public bool canShrink=false;
        public bool ignoreV = true;
        private Transform originParent=null;
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

    }
}