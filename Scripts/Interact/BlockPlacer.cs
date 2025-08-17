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
    public Color canPlaceColor = Color.red;
    public Color cannotPlaceColor = Color.red;
    public int maxCount = 1;         // 每关最多放几个
    public LayerMask groundMask;     // 地面层（可选，限制只能放在地面）

    private int remaining;
    private SpriteRenderer previewRenderer;
    void Awake()
    {
        remaining = maxCount;
        //blockPrefab.GetComponent<SpriteRenderer>().color = new Color(0, 255, 255, 20);
        // 创建预览精灵
        GameObject previewObj = new GameObject("BlockPreview");
        previewObj.transform.SetParent(transform);
        previewRenderer = previewObj.AddComponent<SpriteRenderer>();
        previewRenderer.sprite = previewSprite;
        previewRenderer.sortingOrder = 0;
        previewRenderer.color = new Color(0, 255, 255, 20);
        previewRenderer.enabled = false;
        BoxCollider2D box = blockPrefab.GetComponent<BoxCollider2D>();
        if (box != null)
        {
            previewRenderer.size = box.size;
        }
    }

    void Update()
    {
        if (!GameManager.Instance.CheckCapability(Capability.PlaceForwardZone))return;
        bool isPreForward = GameManager.Instance.currentState== GameState.PreForwardTime;
        bool canShow = !(GameManager.Instance.timeStopped || !isPreForward || remaining <= 0);
        previewRenderer.enabled = canShow;
        if (!canShow) return;

        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 cell = SnapToGrid(world);
        previewRenderer.transform.position = cell;

        bool canPlace = checkMaskOverlap(cell,groundMask);
        previewRenderer.color = canPlace ? canPlaceColor : cannotPlaceColor;

        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            Instantiate(blockPrefab, cell, Quaternion.identity);
            remaining--;
        }
    }
    bool checkMaskOverlap(Vector3 cell, LayerMask groundMask)
    {
        var c1 = new Vector3(cell.x-0.1f, cell.y+0.1f, 0);
        var c2 = new Vector3(cell.x+0.1f, cell.y-0.1f, 0);
        return !Physics2D.OverlapPoint(c1, groundMask) && !Physics2D.OverlapPoint(c2, groundMask); ;
    }
    Vector3 SnapToGrid(Vector3 p)
    {
        p.z = 0;
        p.x = Mathf.Round(p.x);
        p.y = Mathf.Round(p.y);
        return p;
    }
}