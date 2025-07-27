using UnityEngine;
using System;
using System.Collections;
using SaveYourself.Core;
using System.Collections.Generic;

public class StartPointTrigger : MonoBehaviour
{
    [Header("属于哪条起点")]
    public bool isReverseStart;   // true = 逆世界起点，false = 正世界起点

    [Header("需要次数")]
    public int requiredCount = 2;

    // 静态计数器
    static int currentCount = 0;
    static Dictionary<string,bool> dic = new();
    void OnTriggerEnter2D(Collider2D c)
    {
        string tag;
        // 判断角色 Tag
        bool isReversePlayer = c.CompareTag("GhostPlayer");
        bool isForwardPlayer = c.CompareTag("Player");
        if (isReversePlayer)
        {
            tag = "GhostPlayer";
        }
        else
        {
            tag = "Player";
        }
        // 逆角色到逆起点 或 正角色到正起点
        if ((isReverseStart && isForwardPlayer) ||
            (!isReverseStart && isReversePlayer))
        {
            if (!dic.ContainsKey(tag))
            {
                currentCount++;
                dic.Add(tag, true);
                Debug.LogFormat("{0} collide ,count {1}", tag,currentCount );
            }
            if (currentCount >= requiredCount)
            {
                // 计数完成
                currentCount = 0;
                dic.Clear();
                TimeManager.Instance.Clear();
                GameManager.instance.Clear();
                GameManager.instance.LoadNextScene();
            }
        }
    }
}