using UnityEngine;
using UnityEngine.Events;

public class CloseOnClick : MonoBehaviour
{
    public UnityEvent onClick;   // �ϣ�RealPanel.SetActive(false)

    // ��� BlockerPanel ʱ����
    public void OnPointerClick()
    {
        onClick.Invoke();
    }
}