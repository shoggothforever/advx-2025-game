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
    public CanvasGroup loadingCanvas;   // �� CanvasGroup
    public Slider progressBar;     // ��ѡ
    public bool isReady = false;
    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        // 1. ��ʼ��ȫ�ֹ�����
        InitManagers();
        DontDestroyOnLoad(loadingCanvas.gameObject);
        DontDestroyOnLoad(progressBar.gameObject);
        // 2. ֱ�ӽ��뿪ʼ�˵�
        LoadScene("MainMenu");
    }

    void InitManagers()
    {
        // ���� AddComponent ���� Addressable ʵ����
        //gameObject.AddComponent<AudioManager>();

        gameObject.AddComponent<GameManager>();
        gameObject.AddComponent<TimeManager>();
    }

    /* ���� API���κεط����ܵ��� */
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadAsync(sceneName));
    }

    IEnumerator LoadAsync(string sceneName)
    {
        loadingCanvas.alpha = 1f;           // ��ʾ���� UI
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