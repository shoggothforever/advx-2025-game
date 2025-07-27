using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SaveYourself.Core;
using static LoaderManager;
public class LevelManager : MonoBehaviour
{
    public LevelConfig config; // Inspector 拖进来

    void Start()
    {
        SpawnAll();
    }
    void SpawnAll()
    {
        if (config == null) { Debug.LogError("忘记挂 LevelConfig！"); return; }
        GameManager.instance.levelName = config.levelName;
        GameManager.instance.nextLevelName = config.nextLevelName;
        GameManager.instance.timeLimit = config.timeLimit;
        // 生成所有物件
        int cnt = 0;
        foreach (var i in config.items)
        {
            GameObject go = Instantiate(i.prefab, i.position, Quaternion.Euler(i.rotation));
            go.transform.localScale = i.scale;
            if (go.name.StartsWith("BeginPos"))
            {
                Debug.Log("find BeginPos");
                GameManager.instance.pastWorld = go;
                cnt++;
            }
            else if (go.name.StartsWith("Player Variant"))
            {
                Debug.Log("find Player Variant");
                GameManager.instance.pastPlayer = go;
                go.transform.SetParent(GameManager.instance.pastWorld.transform, true);
                TimeManager.Instance.reversePlayer = go;
                go.SetActive(false);
                ++cnt;
            }
            else if (go.name.StartsWith("EndPos"))
            {
                Debug.Log("find EndPos");
                GameManager.instance.reverseWorld = go;
                ++cnt;
            }
            else if (go.name.StartsWith("Player reverse"))
            {
                Debug.Log("find Player reverse");
                go.transform.SetParent(GameManager.instance.reverseWorld.transform, true);
                GameManager.instance.reversePlayer = go;
                TimeManager.Instance.reversePlayer = go;
                var vircam = go.GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>();
                if (vircam != null)
                {
                    Debug.Log("find virtual cinemachine");
                    GameManager.instance.reverseVirtualCamera = vircam;
                }
                ++cnt;
            }
            else if (go.name.StartsWith("waterTransformers"))
            {
                Debug.Log("find waterTransformers");
                GameManager.instance.waterTransformers = go.GetComponent<WaterTransformer[]>();
            }else if (go.name.StartsWith("Canvas"))
            {
                Debug.Log("find waterTransformers");
                go.GetComponent<Canvas>().worldCamera=Camera.main;
                GameManager.instance.countdownText=go.GetComponentInChildren<Text>();
                ++cnt;
            }
        }
        GameManager.instance.boxes= GameObject.FindGameObjectsWithTag("Box");
        if (GameManager.instance.boxes != null)
        {
            Debug.Log("find reversible boxes");
        }
        if(cnt>=2)LoaderManager.Instance.isReady = true;
    }
    public void LoadNextLevel()
    {
        if (config == null) { Debug.LogError("忘记挂 LevelConfig！"); return; }
        LoaderManager.Instance.isReady = false;
        LoaderManager.Instance.LoadScene(config.nextLevelName);
    }
    public void ReloadLevel()
    {
        if (config == null) { Debug.LogError("忘记挂 LevelConfig！"); return; }
        LoaderManager.Instance.isReady = false;
        LoaderManager.Instance.LoadScene(config.levelName);
    }
    public void OnQuitClick()
    {
        Application.Quit();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            LoadNextLevel();
        }
    }

}
