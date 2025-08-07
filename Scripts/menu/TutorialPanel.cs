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
    public string videoFileName = "jump.mp4";
    [TextArea(3, 8)]
    public string explanation;         // 在 Inspector 里填中文

    private void Start()
    {
        // 设置视频
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = Path.Combine(Application.streamingAssetsPath, "Tutorial", videoFileName);
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.isLooping = false;
        videoPlayer.loopPointReached += OnVideoEnd; // 注册
        explanationText.text = explanation;
    }
    private void OnEnable()
    {
        root.SetActive(true);
        videoPlayer.Stop();
        videoPlayer.Play();
    }

    private void OnDisable()
    {
        videoPlayer.Stop();
        root.SetActive(false);
    }
    private void OnVideoEnd(VideoPlayer vp)
    {
        vp.time = 0;                 // 回到 0 秒
                                     // 如果你只想停在首帧而不立刻重播，可在这里再 Pause
    }
    public void OnClickClose()
    {
        // 可以播放下一段或直接关闭
        gameObject.SetActive(false);
    }
}