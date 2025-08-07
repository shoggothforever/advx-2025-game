using UnityEngine;
using UnityEngine.Video;
using TMPro;
using System.IO;
using UnityEngine.UI;

public class TutorialPanel : MonoBehaviour
{
    [Header("UI")]
    public GameObject root;            // TutorialCanvas
    public VideoPlayer videoPlayer;    // ��һ�� VideoPlayer ���
    public RenderTexture renderTexture;
    public Text explanationText;
    public Button nextButton;

    [Header("����")]
    public string videoFileName = "Resource/Video/jump.mp4";
    [TextArea(3, 8)]
    public string explanation;         // �� Inspector ��������

    private void Awake()
    {
        // �󶨰�ť
        nextButton.onClick.AddListener(OnNext);

        // ������Ƶ
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
        // ���Բ�����һ�λ�ֱ�ӹر�
        gameObject.SetActive(false);
    }
}