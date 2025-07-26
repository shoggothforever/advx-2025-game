using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Core;
public class WaterTransformer : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject IceArea;
    public GameObject WaterArea;
    public GameObject SteamArea;
    public bool isOnFire = true;
    // Update is called once per frame
    public void Start()
    {
        if(IceArea != null || SteamArea != null)
        {
            isOnFire = true;
        }
        if(IceArea)IceArea.SetActive(false);
        if(WaterArea)WaterArea.SetActive(true);
        if(SteamArea)SteamArea.SetActive(false);
    }
    public void changeWater()
    {
        if (GameManager.instance.currentState == GameState.ReverseTime)
        {
            if(IceArea)IceArea.SetActive(true);
            if(WaterArea) WaterArea.SetActive(false);
            if(SteamArea) SteamArea.SetActive(false);
        }else if(GameManager.instance.currentState == GameState.ForwardTime)
        {
            if (IceArea) IceArea.SetActive(false);
            if (WaterArea) WaterArea.SetActive(false);
            if (SteamArea) SteamArea.SetActive(true);
        }
    }
}
