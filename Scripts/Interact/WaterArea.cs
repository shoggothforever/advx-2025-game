using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WaterArea : MonoBehaviour
{
    [Header("水体参数")]
    public float density = 20f;   // 水密度 kg/m³，可调
    public float linearDrag = 2f;  // 水平/垂直线性阻力
    public float angularDrag = 1f;  // 角阻力

    // 用于 Overlap 的缓存
    readonly List<Collider2D> inside = new List<Collider2D>();
    readonly ContactFilter2D filter = new ContactFilter2D().NoFilter();

    Collider2D waterCol;

    void Awake() => waterCol = GetComponent<Collider2D>();

    void FixedUpdate()
    {
        // 1. 找出所有位于触发器内的 Collider
        Physics2D.OverlapCollider(waterCol, filter, inside);

        foreach (var col in inside)
        {
            if (!col.attachedRigidbody) continue;

            var rb = col.attachedRigidbody;
            var body = rb.gameObject.GetComponent<FloatBox>();

            // 2. 如果物体没有 BuoyantBody 组件，自动给它加一份
            if (!body)
                body = rb.gameObject.AddComponent<FloatBox>();

            // 3. 计算浸入比例并施加浮力
            body.ApplyBuoyancy(this, waterCol);
        }
    }
}