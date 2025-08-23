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
    [Header("物体参数")]
    public float objectDensity = 500f;  // 物体自身密度，决定沉浮
    Rigidbody2D rb;
    Collider2D col;
    float defaultGravity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultGravity = rb.gravityScale;
        col = GetComponent<Collider2D>();
    }
    // 由 WaterArea 每帧调用
    public void ApplyBuoyancy(WaterArea water, Collider2D waterCol)
    {
        // 1. 计算浸入面积
        float submergedArea = GetSubmergedArea(waterCol);

        // 2. 阿基米德浮力
        float displacedMass = submergedArea * 1f * water.density;
        float weight = rb.mass * -Physics2D.gravity.y;
        float buoyantForce = displacedMass * -Physics2D.gravity.y;

        // 3. 合力 + 阻尼
        rb.AddForce(Vector2.up * (buoyantForce - weight), ForceMode2D.Force);

        // 简单线性/角阻尼，防止抖动
        rb.velocity *= 1f - water.linearDrag * Time.fixedDeltaTime;
        rb.angularVelocity *= 1f - water.angularDrag * Time.fixedDeltaTime;
    }
    float GetSubmergedArea(Collider2D waterCol)
    {
        // 2D 快速近似：用 Bounds 相交高度 * 宽度
        Bounds waterB = waterCol.bounds;
        Bounds objB = col.bounds;

        float overlapY = Mathf.Max(0,
            Mathf.Min(waterB.max.y, objB.max.y) -
            Mathf.Max(waterB.min.y, objB.min.y));

        float overlapX = Mathf.Max(0,
            Mathf.Min(waterB.max.x, objB.max.x) -
            Mathf.Max(waterB.min.x, objB.min.x));

        return overlapX * overlapY;
    }
    //void FixedUpdate()
    //{
    //    // 计算箱子底部到水面的距离
    //    float bottomY = rb.position.y - GetComponent<Collider2D>().bounds.extents.y;
    //    float submerge = waterLevelY - bottomY;

    //    // 只有浸入部分才给浮力
    //    if (submerge > 0)
    //    {
    //        float force = submerge * buoyancy;
    //        rb.AddForce(Vector2.up * force, ForceMode2D.Force);

    //        // 垂直阻尼：让上下抖动尽快稳定
    //        rb.velocity = new Vector2(rb.velocity.x,
    //                                  rb.velocity.y * (1 - damping * Time.fixedDeltaTime));
    //    }
    //}
}