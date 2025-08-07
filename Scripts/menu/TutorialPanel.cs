using UnityEngine;
using UnityEngine.Video;
using TMPro;
using System.IO;
using UnityEngine.UI;

public class TutorialPanel : MonoBehaviour
{
    [Header("UI")]
    public GameObject root;            // TutorialCanvas
    public VideoPlayer videoPlayer;    // 拖一个 VideoPlayer 组件
    public RenderTexture renderTexture;
    public Text explanationText;
    public Button nextButton;

    [Header("内容")]
    public string videoFileName = "Resource/Video/jump.mp4";
    [TextArea(3, 8)]
    public string explanation;         // 在 Inspector 里填中文

    private void Awake()
    {
        // 绑定按钮
        nextButton.onClick.AddListener(OnNext);

        // 设置视频
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = Path.Combine(Application.streamingAssetsPath, "Tutorial", videoFileName);
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.isLooping = false;

        explanationText.text = explanation;
    }

    private void OnEnable()
    {
        root.SetActive(true);
        videoPlayer.Play();
    }

    private void OnDisable()
    {
        videoPlayer.Stop();
        root.SetActive(false);
    }

    void OnNext()
    {
        // 可以播放下一段或直接关闭
        gameObject.SetActive(false);
    }
}