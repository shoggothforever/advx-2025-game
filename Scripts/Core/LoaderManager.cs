using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;
using SaveYourself.Core;
using SaveYourself.Mechanics;
public class LoaderManager : MonoBehaviour
{
    public static LoaderManager Instance { get; private set; }
    public string startLevel = "MainMenu";
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

    }
    void Start()
    {
        //直接进入开始菜单
        LoadScene(startLevel);
    }

    void InitManagers()
    {
        // 按需 AddComponent 或用 Addressable 实例化
        //gameObject.AddComponent<AudioManager>();

        gameObject.AddComponent<GameManager>();
        gameObject.AddComponent<TimeManager>();
        gameObject.AddComponent<SaveManager>();
    }

    /* 公开 API，任何地方都能调用 */
    public void LoadScene(string sceneName)
    {
        if (SaveManager.Instance.Data.levels.ContainsKey(sceneName))
        {
            StartCoroutine(LoadAsync(sceneName));
        }
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
    public static T[] FindComponentsInScene<T>(string sceneName) where T : Component
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return scene.isLoaded
            ? scene.GetRootGameObjects()
                   .SelectMany(go => go.GetComponentsInChildren<T>(true))
                   .ToArray()
            : new T[0];
    }
}