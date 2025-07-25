using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraAdaption : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Camera.main.orthographicSize = Screen.height / 2f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
