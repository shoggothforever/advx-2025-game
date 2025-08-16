using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Core;
public class BlockPlacer : MonoBehaviour
{
    [Header("放置参数")]
    public GameObject blockPrefab;   // StopBlock 预制体
    [Header("预览精灵")]
    public Sprite previewSprite;
    public Color canPlaceColor = Color.green;
    public Color cannotPlaceColor = Color.red;
    public int maxCount = 3;         // 每关最多放几个
    public LayerMask groundMask;     // 地面层（可选，限制只能放在地面）

    private int remaining;
    private SpriteRenderer previewRenderer;
    void Awake()
    {
        remaining = maxCount;
        blockPrefab.GetComponent<SpriteRenderer>().color = new Color(0, 255, 255, 50);
        // 创建预览精灵
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