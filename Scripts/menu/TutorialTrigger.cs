using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public GameObject panel;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("GhostPlayer"))
        {
            Debug.Log("play tutorial");
            panel.SetActive(true);
        }
    }
}
