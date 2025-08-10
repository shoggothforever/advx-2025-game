using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TutorialTrigger : MonoBehaviour
{
    public GameObject panel;
    public GameObject bubble;
    public Text tipsText;
    public float showDistance = 2f;
    public bool inTrigger = false;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("GhostPlayer"))
        {
            bubble.SetActive(true);
            inTrigger = true;
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("GhostPlayer"))
        {
            inTrigger = false;
            bubble.SetActive(false);
            StopAllCoroutines();
            if (panel.activeSelf) SetTut(false);
        }
    }
    private void Update()
    {
        if (panel.activeSelf)
        {
            bubble.SetActive(false);
        }
        else
        {
            bubble.SetActive(true);
        }
        if (!panel.activeSelf && inTrigger && Input.GetKeyDown(KeyCode.F))
        {
                SetTut(true);
        }else if(panel.activeSelf && Input.GetKeyDown(KeyCode.F))
        {
            SetTut(false);
        }

    }

    private void SetTut(bool val)
    {
        panel.SetActive(val);
    }
}
