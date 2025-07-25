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
        //var gos = gameObject.GetComponentsInChildren<GameObject>();
        //foreach(var go in gos)
        //{
        //    if (go.name == "IceArea")
        //    {
        //        IceArea = go;
        //        Debug.Log("find IceArea");

        //    }
        //    else if(go.name == "WaterArea")
        //    {
        //        WaterArea = go;
        //        Debug.Log("find WaterArea");

        //    }
        //    else if (go.name == "SteamArea")
        //    {
        //        SteamArea = go;
        //        Debug.Log("find SteamArea");
        //    }
        //}
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
            IceArea.SetActive(true);
            WaterArea.SetActive(false);
            SteamArea.SetActive(false);
        }else if(GameManager.instance.currentState == GameState.ForwardTime)
        {
            IceArea.SetActive(false);
            WaterArea.SetActive(false);
            SteamArea.SetActive(true);
        }
    }
}
