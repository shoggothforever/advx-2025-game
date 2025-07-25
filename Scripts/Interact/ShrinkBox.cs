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
            var newBox = Instantiate(ShrinkBoxPrefab, transform.position, transform.rotation);
            var newRb = newBox.GetComponent<Rigidbody2D>();
            //newRb.velocity = rb.velocity;
            //newRb.angularVelocity = rb.angularVelocity;

            Destroy(gameObject);
        }
        void Awake()
        {
            canShrink = true;
        }
    }
}