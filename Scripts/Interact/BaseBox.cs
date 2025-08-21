using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Core;


public class BaseBox :MonoBehaviour
{
    public Rigidbody2D rb;

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
            // 1. 判断玩家是否站在平台顶部
            ContactPoint2D[] contacts = collision.contacts;
            foreach (var cp in contacts)
            {
                // 玩家碰撞点法线朝上 => 玩家脚底接触平台顶面
                if (Vector2.Dot(cp.normal, Vector2.up) > 0.7f)
                {
                    originParent = collision.collider.transform.parent;
                    collision.collider.transform.SetParent(transform, true);
                    break;
                }
            }
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