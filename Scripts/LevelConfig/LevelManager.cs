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
        Debug.LogFormat("Player :{0}", LayerMask.NameToLayer("Player"));
        Debug.LogFormat("GhostPlayer :{0}", LayerMask.NameToLayer("GhostPlayer"));
        Debug.LogFormat("Box :{0}", LayerMask.NameToLayer("Box"));
        Debug.LogFormat("Player :{0}", LayerMask.NameToLayer("Player"));
    }
    void SpawnAll()
    {
        if (config == null) { Debug.LogError("忘记挂 LevelConfig！"); return; }
        GameManager.Instance.levelName = config.levelName;
        GameManager.Instance.nextLevelName = config.nextLevelName;
        GameManager.Instance.timeLimit = config.timeLimit;
        // 生成所有物件
        int cnt = 0;
        foreach (var i in config.items)
        {
            GameObject go = Instantiate(i.prefab, i.position, Quaternion.Euler(i.rotation));
            go.transform.localScale = i.scale;
            if (go.name.StartsWith("BeginPos"))
            {
                Debug.Log("find BeginPos");
                GameManager.Instance.pastWorld = go;
                cnt++;
            }
            else if (go.name.StartsWith("Player Variant"))
            {
                Debug.Log("find Player Variant");
                GameManager.Instance.pastPlayer = go;
                go.transform.SetParent(GameManager.Instance.pastWorld.transform, true);
                TimeManager.Instance.reversePlayer = go;
                go.SetActive(false);
                ++cnt;
            }
            else if (go.name.StartsWith("EndPos"))
            {
                Debug.Log("find EndPos");
                GameManager.Instance.reverseWorld = go;
                ++cnt;
            }
            else if (go.name.StartsWith("Player reverse"))
            {
                Debug.Log("find Player reverse");
                go.transform.SetParent(GameManager.Instance.reverseWorld.transform, true);
                GameManager.Instance.reversePlayer = go;
                TimeManager.Instance.reversePlayer = go;
                var vircam = go.GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>();
                if (vircam != null)
                {
                    Debug.Log("find virtual cinemachine");
                    GameManager.Instance.reverseVirtualCamera = vircam;
                }
                ++cnt;
            }
            else if (go.name.StartsWith("waterTransformer"))
            {
                Debug.Log("find waterTransformers");
                GameManager.Instance.waterTransformers = go.GetComponent<List<WaterTransformer>>();
            }else if (go.name.StartsWith("Canvas"))
            {
                Debug.Log("find Canvas");
                go.GetComponent<Canvas>().worldCamera=Camera.main;
                GameManager.Instance.countdownText=go.GetComponentInChildren<Text>();
                ++cnt;
            }
        }
        GameManager.Instance.boxes= GameObject.FindGameObjectsWithTag("Box");
        if (GameManager.Instance.boxes != null)
        {
            Debug.Log("find reversible boxes");
        }
        if(cnt>=2)LoaderManager.Instance.isReady = true;
    }
    public void LoadNextLevel()
    {
        if (config == null) { Debug.LogError("忘记挂 LevelConfig！"); return; }
        Instance.isReady = false;
        TimeManager.Instance.Clear();
        GameManager.Instance.Clear();
        Instance.LoadScene(config.nextLevelName);
    }
    public void ReloadLevel()
    {
        if (config == null) { Debug.LogError("忘记挂 LevelConfig！"); return; }
        Instance.isReady = false;
        Instance.LoadScene(config.levelName);
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
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (config.levelName != "MainMenu")
            {
                Instance.LoadScene("MainMenu");
            }
            else Application.Quit();
        }
    }
}
