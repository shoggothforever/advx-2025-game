using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Core;
public class DeathZone : MonoBehaviour
{
    public void OnCollisionEnter2D(Collision2D c)
    {
        if(c.collider.CompareTag("GhostPlayer"))
        {
            LoaderManager.Instance.LoadScene(GameManager.Instance.levelName); 
        }else if (c.collider.CompareTag("Player"))
        {
            GameManager.Instance.RestartFromPreForward();
        }
    }
}
