using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SaveYourself.Core;
using SaveYourself.Utils;
using SaveYourself.Model;
using static LoaderManager;
using UnityEngine.EventSystems;

public class LevelManager : MonoBehaviour
{
    public LevelConfig config;
    public GameObject PauseMenu;
    public ILevelLogic level;
    void Awake()
    { 
        string scene = gameObject.scene.name;
        config = Resources.Load<LevelConfig>($"Configs/{scene}");
    }
    void Start() 
    {
        if(PauseMenu!=null)SetPasueMenu(false);
        SpawnAll();
    }
    void SpawnAll()
    {
        if (config == null) { Debug.LogError("无法找到 LevelConfig！"); return; }
        level = LevelFactory.Create(config.levelName);
        GameManager.Instance.levelName = config.levelName;
        GameManager.Instance.nextLevelName = config.nextLevelName;
        GameManager.Instance.timeLimit = config.timeLimit;
        GameManager.Instance.lm = this;
        // 生成所有物件
        int cnt = 0;
        foreach (var i in config.items)
        {
            GameObject go = Instantiate(i.prefab, i.position, Quaternion.Euler(i.rotation));
            go.transform.localScale = i.scale;
            if (go.name.StartsWith("BeginPos"))
            {
                GameManager.Instance.pastWorld = go;
                cnt++;
            }
            else if (go.name.StartsWith("Player_Variant"))
            {
                GameManager.Instance.pastPlayer = go;
                GameManager.Instance.originPastPlayPosition = i.position;
                go.transform.SetParent(GameManager.Instance.pastWorld.transform, true);
                TimeManager.Instance.reversePlayer = go;
                go.SetActive(false);
                ++cnt;
            }
            else if (go.name.StartsWith("EndPos"))
            {
                GameManager.Instance.reverseWorld = go;
                ++cnt;
            }
            else if (go.name.StartsWith("Player_reverse"))
            {
                go.transform.SetParent(GameManager.Instance.reverseWorld.transform, true);
                GameManager.Instance.reversePlayer = go;
                TimeManager.Instance.reversePlayer = go;
                var vircam = go.GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>();
                if (vircam != null)
                {
                    GameManager.Instance.reverseVirtualCamera = vircam;
                }
                ++cnt;
            }
            else if (go.name.StartsWith("waterTransformer"))
            {
                GameManager.Instance.waterTransformers = go.GetComponent<List<WaterTransformer>>();
            }else if (go.name.StartsWith("Canvas"))
            {
                go.GetComponent<Canvas>().worldCamera=Camera.main;
                GameManager.Instance.countdownText=go.GetComponentInChildren<Text>();
                ++cnt;
            }
        }
        if (FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();   // 2D/3D UI 都需要
            #if ENABLE_INPUT_SYSTEM
            es.AddComponent<InputSystemUIInputModule>(); // 新版 Input System
            #endif
        }
        if (cnt>=2)Instance.isReady = true;
        level.DoBeforeLevel();
    }
    public void LoadNextLevel()
    {
        if (config == null) { Debug.LogError("忘记挂 LevelConfig！"); return; }
        Instance.isReady = false;
        TimeManager.Instance.Clear();
        GameManager.Instance.Clear();
        level.DoAfterLevel();
        Instance.LoadScene(config.nextLevelName);
    }
    public void OnResume()
    {
        SetPasueMenu(false);
    }
    public void ReloadLevel()
    {
        if (config == null) { Debug.LogError("忘记挂 LevelConfig！"); return; }
        SetPasueMenu(false);
        Instance.LoadScene(config.levelName);
    }
    public void ReturnToMid()
    {
        if (config == null) { Debug.LogError("忘记挂 LevelConfig！"); return; }
        SetPasueMenu(false);
        GameManager.Instance.RestartFromPreForward();
    }
    public void OnBackToMainMenu()
    {
        SetPasueMenu(false);
        Instance.LoadScene("MainMenu");
    }
    public void OnQuitClick()
    {
        SetPasueMenu(false);
        Application.Quit();
    }
    public void SetPasueMenu(bool val)
    {
        GameManager.Instance.controlTime(val);
        PauseMenu.SetActive(val);
    }
    private void Update()
    {
        #if UNITY_EDITOR 
        if (Input.GetKeyDown(KeyCode.N))
        {
            LoadNextLevel();
        }
        #endif
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (config.levelName != "MainMenu")
            {
                if (PauseMenu.activeSelf)
                {
                    OnResume();
                    return;
                }
                SetPasueMenu(true);
            }
            else Application.Quit();
        }
    }
}
