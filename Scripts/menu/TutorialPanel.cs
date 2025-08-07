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
    public string videoFileName = "jump.mp4";
    [TextArea(3, 8)]
    public string explanation;         // �� Inspector ��������

    private void Start()
    {
        // ������Ƶ
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = Path.Combine(Application.streamingAssetsPath, "Tutorial", videoFileName);
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.isLooping = false;
        videoPlayer.loopPointReached += OnVideoEnd; // ע��
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
        vp.time = 0;                 // �ص� 0 ��
                                     // �����ֻ��ͣ����֡���������ز������������� Pause
    }
    public void OnClickClose()
    {
        // ���Բ�����һ�λ�ֱ�ӹر�
        gameObject.SetActive(false);
    }
}