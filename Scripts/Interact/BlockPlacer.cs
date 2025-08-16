using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Core;
public class BlockPlacer : MonoBehaviour
{
    [Header("���ò���")]
    public GameObject blockPrefab;   // StopBlock Ԥ����
    [Header("Ԥ������")]
    public Sprite previewSprite;
    public Color canPlaceColor = Color.green;
    public Color cannotPlaceColor = Color.red;
    public int maxCount = 3;         // ÿ�����ż���
    public LayerMask groundMask;     // ����㣨��ѡ������ֻ�ܷ��ڵ��棩

    private int remaining;
    private SpriteRenderer previewRenderer;
    void Awake()
    {
        remaining = maxCount;
        blockPrefab.GetComponent<SpriteRenderer>().color = new Color(0, 255, 255, 50);
        // ����Ԥ������
        GameObject previewObj = new GameObject("BlockPreview");
        previewObj.transform.SetParent(transform);
        previewRenderer = previewObj.AddComponent<SpriteRenderer>();
        previewRenderer.sprite = previewSprite;
        previewRenderer.sortingOrder = 1000;
        BoxCollider2D box = blockPrefab.GetComponent<BoxCollider2D>();
        if (box != null)
        {
            previewRenderer.size = box.size;
        }
    }

    void Update()
    {
        bool isPreForward = GameManager.Instance.currentState== GameState.PreForwardTime;
        previewRenderer.enabled = isPreForward && remaining > 0;

        if (!isPreForward || remaining <= 0) return;

        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 cell = SnapToGrid(world);
        previewRenderer.transform.position = cell;

        bool canPlace = !Physics2D.OverlapPoint(cell, groundMask);
        previewRenderer.color = canPlace ? canPlaceColor : cannotPlaceColor;

        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            Instantiate(blockPrefab, cell, Quaternion.identity);
            remaining--;
        }
    }

    Vector3 SnapToGrid(Vector3 p)
    {
        p.z = 0;
        p.x = Mathf.Round(p.x);
        p.y = Mathf.Round(p.y);
        return p;
    }
}