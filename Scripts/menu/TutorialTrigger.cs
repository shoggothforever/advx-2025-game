using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public GameObject panel;
    private bool tutStart = false;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!tutStart&& other.CompareTag("Player") || other.CompareTag("GhostPlayer"))
        {
            Debug.Log("play tutorial");
            panel.SetActive(true);
            tutStart = true;
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (tutStart && other.CompareTag("Player") || other.CompareTag("GhostPlayer"))
        {
            Debug.Log("stop tutorial");
            panel.SetActive(false);
            tutStart = false;
        }
    }
}
