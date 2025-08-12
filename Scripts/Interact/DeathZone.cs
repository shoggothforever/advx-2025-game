using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Core;
public class DeathZone : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D c)
    {
        Debug.Log("trigger death zone");
        if (c.CompareTag("GhostPlayer"))
        {
            Debug.Log("you die ");
            LoaderManager.Instance.LoadScene(GameManager.Instance.levelName);
        }
        else if (c.CompareTag("Player"))
        {
            GameManager.Instance.RestartFromPreForward();
        }
    }
    public void OnCollision2D(Collision2D c)
    {
        Debug.Log("collision death zone");
        if (c.collider.CompareTag("GhostPlayer"))
        {
            Debug.Log("you die ");
            LoaderManager.Instance.LoadScene(GameManager.Instance.levelName);
        }
        else if (c.collider.CompareTag("Player"))
        {
            GameManager.Instance.RestartFromPreForward();
        }
    }
}
