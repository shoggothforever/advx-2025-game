using UnityEngine;
using UnityEngine.Events;

public class CloseOnClick : MonoBehaviour
{
    public UnityEvent onClick;   // ÍÏ£ºRealPanel.SetActive(false)

    // µã»÷ BlockerPanel Ê±´¥·¢
    public void OnPointerClick()
    {
        onClick.Invoke();
    }
}