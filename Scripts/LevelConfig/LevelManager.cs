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
            if (go.name.StartsWith("Player Variant"))
            {
                Debug.Log("find Player Variant");
                GameManager.instance.pastPlayer = go;
                TimeManager.Instance.reversePlayer = go;
                go.SetActive(false);
            }
            else if (go.name.StartsWith("EndPos"))
            {
                Debug.Log("find EndPos");

                GameManager.instance.reverseWorld = go;
            }
            else if (go.name.StartsWith("Player reverse"))
            {
                Debug.Log("find Player reverse");

                go.transform.SetParent(GameManager.instance.reverseWorld.transform, true);
                GameManager.instance.reversePlayer = go;
                TimeManager.Instance.reversePlayer = go;

            }
            else if (go.name.StartsWith("waterTransformers"))
            {
                Debug.Log("find waterTransformers");

                GameManager.instance.waterTransformers = go.GetComponent<WaterTransformer[]>();
            }
        }
        LoaderManager.Instance.isReady = true;
        //var brain = Camera.main.GetComponent<Cinemachine.CinemachineBrain>();

        // 把玩家虚拟相机设为 Live
    }
    public void LoadNextLevel()
    {
        if (config == null) { Debug.LogError("忘记挂 LevelConfig！"); return; }
        LoaderManager.Instance.isReady = false;
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
