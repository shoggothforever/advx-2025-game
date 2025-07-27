using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Collider2D))]
public class WindZone : MonoBehaviour
{
    [Header("��������")]
    [Tooltip("��ֵ���ң���ֵ����")]
    public float force = 10f;

    [Tooltip("Ӱ��� Layer")]
    public LayerMask affectedLayers = -1;   // -1 ��ʾ���в�

    readonly List<Rigidbody2D> inside = new();

    void OnTriggerEnter2D(Collider2D c)
    {
        Rigidbody2D rb = c.attachedRigidbody;
        if (rb && ((1 << rb.gameObject.layer) & affectedLayers) != 0)
            inside.Add(rb);
    }

    void OnTriggerExit2D(Collider2D c)
    {
        Rigidbody2D rb = c.attachedRigidbody;
        if (rb) inside.Remove(rb);
    }

    void FixedUpdate()
    {
        foreach (var rb in inside)
            rb.AddForce(Vector2.right * force, ForceMode2D.Force);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = force > 0 ? Color.red : Color.blue;
        Gizmos.DrawWireCube(GetComponent<Collider2D>().bounds.center,
                            GetComponent<Collider2D>().bounds.size);
    }
#endif
}