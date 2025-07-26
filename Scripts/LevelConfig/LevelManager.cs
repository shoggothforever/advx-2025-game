using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Core;
using static LoaderManager;
public class LevelManager : MonoBehaviour
{
    public LevelConfig config; // Inspector 拖进来

    void Awake()
    {
        SpawnAll();
    }

    void SpawnAll()
    {
        if (config == null) { Debug.LogError("忘记挂 LevelConfig！"); return; }
        // 生成所有物件
        foreach (var i in config.items)
        {
            GameObject go = Instantiate(i.prefab, i.position, Quaternion.Euler(i.rotation));
            go.transform.localScale = i.scale;
        }
    }
    public void LoadNextLevel()
    {
        if (config == null) { Debug.LogError("忘记挂 LevelConfig！"); return; }
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
