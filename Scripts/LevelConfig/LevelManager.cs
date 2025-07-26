using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Core;
using static LoaderManager;
public class LevelManager : MonoBehaviour
{
    public LevelConfig config; // Inspector �Ͻ���

    void Awake()
    {
        SpawnAll();
    }

    void SpawnAll()
    {
        if (config == null) { Debug.LogError("���ǹ� LevelConfig��"); return; }
        // �����������
        foreach (var i in config.items)
        {
            GameObject go = Instantiate(i.prefab, i.position, Quaternion.Euler(i.rotation));
            go.transform.localScale = i.scale;
        }
    }
    public void LoadNextLevel()
    {
        if (config == null) { Debug.LogError("���ǹ� LevelConfig��"); return; }
        LoaderManager.Instance.LoadScene(config.nextLevelName);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            LoadNextLevel();
        }
    }
}
