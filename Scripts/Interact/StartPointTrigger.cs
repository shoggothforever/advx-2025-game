using UnityEngine;
using System;
using System.Collections;
using SaveYourself.Core;
using System.Collections.Generic;

public class StartPointTrigger : MonoBehaviour
{
    [Header("�����������")]
    public bool isReverseStart;   // true = ��������㣬false = ���������

    [Header("��Ҫ����")]
    public int requiredCount = 2;

    // ��̬������
    static int currentCount = 0;
    static Dictionary<string,bool> dic = new();
    void OnTriggerEnter2D(Collider2D c)
    {
        string tag;
        // �жϽ�ɫ Tag
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
        // ���ɫ������� �� ����ɫ�������
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
                // �������
                currentCount = 0;
                dic.Clear();
                TimeManager.Instance.Clear();
                GameManager.instance.Clear();
                GameManager.instance.LoadNextScene();
            }
        }
    }
}