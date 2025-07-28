using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SaveYourself.Core;

public class MenuManager : MonoBehaviour
{
    public GameObject chooseLevelPanel;
    public MenuManager Instance { get; private set; }
    public void Start()
    {
        if (Instance==null) Instance = this;
    }
    public void OnStartClick()
    {
        GameManager.instance.LoadNextScene();
    }
    public void OnChooseLevelClick()
    {
        chooseLevelPanel.SetActive(true);
    }
    public void OnQuitClick()
    {
        Application.Quit();
    }
    public void Update()
    {
        if (chooseLevelPanel.active && Input.GetKeyUp(KeyCode.Escape))
        {
            chooseLevelPanel.SetActive(false);
        }
    }
}