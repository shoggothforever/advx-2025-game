using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SaveYourself.Core;
using SaveYourself.Mechanics;
public class LoaderManager : MonoBehaviour
{
    public static LoaderManager Instance { get; private set; }

    [Header("UI")]
    public CanvasGroup loadingCanvas;   // 拖 CanvasGroup
    public Slider progressBar;     // 可选
    public bool isReady = false;
    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        // 1. 初始化全局管理器
        InitManagers();
        DontDestroyOnLoad(loadingCanvas.gameObject);
        DontDestroyOnLoad(progressBar.gameObject);
        // 2. 直接进入开始菜单
        LoadScene("MainMenu");
    }

    void InitManagers()
    {
        // 按需 AddComponent 或用 Addressable 实例化
        //gameObject.AddComponent<AudioManager>();

        gameObject.AddComponent<GameManager>();
        gameObject.AddComponent<TimeManager>();
    }

    /* 公开 API，任何地方都能调用 */
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadAsync(sceneName));
    }

    IEnumerator LoadAsync(string sceneName)
    {
        loadingCanvas.alpha = 1f;           // 显示加载 UI
        yield return null;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            if (progressBar) progressBar.value = op.progress;
            yield return null;
        }

        if (progressBar) progressBar.value = 1f;
        yield return new WaitForSeconds(0.2f);

        op.allowSceneActivation = true;
        loadingCanvas.alpha = 0f;
    }
}