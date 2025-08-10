using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SaveYourself.Core;
using static LoaderManager;
using UnityEngine.EventSystems;

public class LevelManager : MonoBehaviour
{
    public LevelConfig config; // Inspector 拖进来
    public GameObject PauseMenu;
    public LevelManager lm;
    public ILevelLogic level;
    void Awake()
    {

        string scene = gameObject.scene.name;
        config = Resources.Load<LevelConfig>($"Configs/{scene}");
    }
    void Start() 
    {
        if (lm == null) lm = this;
        if(PauseMenu!=null)SetPasueMenu(false);
        SpawnAll();
    }
    void SpawnAll()
    {
        if (config == null) { Debug.LogError("无法找到 LevelConfig！"); return; }
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
    }
    public void LoadNextLevel()
    {
        if (config == null) { Debug.LogError("忘记挂 LevelConfig！"); return; }
        Instance.isReady = false;
        TimeManager.Instance.Clear();
        GameManager.Instance.Clear();
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
    private void SetPasueMenu(bool val)
    {
        GameManager.Instance.controlTime(val);
        PauseMenu.SetActive(val);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            LoadNextLevel();
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (PauseMenu.activeSelf)
            {
                OnResume();
            }
            else if (config.levelName != "MainMenu")
            {
                SetPasueMenu(true);
            }
            else Application.Quit();
        }
    }
}
public interface ILevelLogic
{
    public void DoWholeLevel();
    public void DoInPreReverse();
    public void DoInReverse();
    public void DoInPreForward();
    public void DoInForward();
}
public class EmptyLevel : ILevelLogic
{
    public void DoInForward()
    {
        throw new System.NotImplementedException();
    }

    public void DoInPreForward()
    {
        throw new System.NotImplementedException();
    }

    public void DoInPreReverse()
    {
        throw new System.NotImplementedException();
    }

    public void DoInReverse()
    {
        throw new System.NotImplementedException();
    }

    public void DoWholeLevel()
    {
        throw new System.NotImplementedException();
    }
}
public class PlayGroundLevel : ILevelLogic
{
    public void DoInForward()
    {
        throw new System.NotImplementedException();
    }

    public void DoInPreForward()
    {
        throw new System.NotImplementedException();
    }

    public void DoInPreReverse()
    {
        throw new System.NotImplementedException();
    }

    public void DoInReverse()
    {
        throw new System.NotImplementedException();
    }

    public void DoWholeLevel()
    {
        throw new System.NotImplementedException();
    }
}