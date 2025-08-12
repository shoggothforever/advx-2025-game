using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FloatBox : MonoBehaviour
{
    [Header("��������")]
    public float waterLevelY = 0f;     // ˮ�� Y ���꣨���ڳ������ֶ��裩
    public float buoyancy = 15f;    // ����ǿ��
    public float damping = 5f;     // ��ֱ���ᣬ��ֹ��

    Rigidbody2D rb;
    float defaultGravity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultGravity = rb.gravityScale;
    }

    void FixedUpdate()
    {
        // �������ӵײ���ˮ��ľ���
        float bottomY = rb.position.y - GetComponent<Collider2D>().bounds.extents.y;
        float submerge = waterLevelY - bottomY;

        // ֻ�н��벿�ֲŸ�����
        if (submerge > 0)
        {
            float force = submerge * buoyancy;
            rb.AddForce(Vector2.up * force, ForceMode2D.Force);

            // ��ֱ���᣺�����¶��������ȶ�
            rb.velocity = new Vector2(rb.velocity.x,
                                      rb.velocity.y * (1 - damping * Time.fixedDeltaTime));
        }
    }
}