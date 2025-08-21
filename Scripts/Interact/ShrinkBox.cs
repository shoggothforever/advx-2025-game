using SaveYourself.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SaveYourself.Interact
{
    public class ShrinkBox : BaseBox
    {
        [SerializeField] GameObject ShrinkBoxPrefab;   // ÍÏµ½ Inspector
        public void EnlargeStable()
        {
            var upPosition=transform.position+new Vector3(0,0.5f,0);
            var newBox = Instantiate(ShrinkBoxPrefab, upPosition, transform.rotation);
            var newRb = newBox.GetComponent<Rigidbody2D>();
            //newRb.velocity = rb.velocity;
            //newRb.angularVelocity = rb.angularVelocity;

            Destroy(gameObject);
        }
        void Awake()
        {
            canShrink = true;
        }
        public TimeReverse.ActionType GetActionType()
        {
            return TimeReverse.ActionType.Ignore;     // Ã¶¾Ù£ºPosition, AnimatorBool, AnimatorTrigger...
        }
    }
}