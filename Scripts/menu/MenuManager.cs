using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Core;
namespace SaveYourself.menu
{

    public class MenuManager : MonoBehaviour
    {
        public void OnStartClick()
        {
            GameManager.instance.LoadNextScene();
        }
    }
}
