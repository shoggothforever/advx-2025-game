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
    [Header("�������")]
    public float objectDensity = 500f;  // ���������ܶȣ���������
    Rigidbody2D rb;
    Collider2D col;
    float defaultGravity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultGravity = rb.gravityScale;
        col = GetComponent<Collider2D>();
    }
    // �� WaterArea ÿ֡����
    public void ApplyBuoyancy(WaterArea water, Collider2D waterCol)
    {
        // 1. ����������
        float submergedArea = GetSubmergedArea(waterCol);

        // 2. �����׵¸���
        float displacedMass = submergedArea * 1f * water.density;
        float weight = rb.mass * -Physics2D.gravity.y;
        float buoyantForce = displacedMass * -Physics2D.gravity.y;

        // 3. ���� + ����
        rb.AddForce(Vector2.up * (buoyantForce - weight), ForceMode2D.Force);

        // ������/�����ᣬ��ֹ����
        rb.velocity *= 1f - water.linearDrag * Time.fixedDeltaTime;
        rb.angularVelocity *= 1f - water.angularDrag * Time.fixedDeltaTime;
    }
    float GetSubmergedArea(Collider2D waterCol)
    {
        // 2D ���ٽ��ƣ��� Bounds �ཻ�߶� * ���
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
    //    // �������ӵײ���ˮ��ľ���
    //    float bottomY = rb.position.y - GetComponent<Collider2D>().bounds.extents.y;
    //    float submerge = waterLevelY - bottomY;

    //    // ֻ�н��벿�ֲŸ�����
    //    if (submerge > 0)
    //    {
    //        float force = submerge * buoyancy;
    //        rb.AddForce(Vector2.up * force, ForceMode2D.Force);

    //        // ��ֱ���᣺�����¶��������ȶ�
    //        rb.velocity = new Vector2(rb.velocity.x,
    //                                  rb.velocity.y * (1 - damping * Time.fixedDeltaTime));
    //    }
    //}
}