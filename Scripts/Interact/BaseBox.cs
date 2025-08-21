using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Core;


public class BaseBox :MonoBehaviour
{
    public Rigidbody2D rb;

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
    private Transform originParent = null;
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            // 1. �ж�����Ƿ�վ��ƽ̨����
            ContactPoint2D[] contacts = collision.contacts;
            foreach (var cp in contacts)
            {
                // �����ײ�㷨�߳��� => ��ҽŵ׽Ӵ�ƽ̨����
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