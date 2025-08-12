using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FloatBox : MonoBehaviour
{
    [Header("浮力参数")]
    public float waterLevelY = 0f;     // 水面 Y 坐标（可在场景里手动设）
    public float buoyancy = 15f;    // 浮力强度
    public float damping = 5f;     // 垂直阻尼，防止振荡

    Rigidbody2D rb;
    float defaultGravity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultGravity = rb.gravityScale;
    }

    void FixedUpdate()
    {
        // 计算箱子底部到水面的距离
        float bottomY = rb.position.y - GetComponent<Collider2D>().bounds.extents.y;
        float submerge = waterLevelY - bottomY;

        // 只有浸入部分才给浮力
        if (submerge > 0)
        {
            float force = submerge * buoyancy;
            rb.AddForce(Vector2.up * force, ForceMode2D.Force);

            // 垂直阻尼：让上下抖动尽快稳定
            rb.velocity = new Vector2(rb.velocity.x,
                                      rb.velocity.y * (1 - damping * Time.fixedDeltaTime));
        }
    }
}