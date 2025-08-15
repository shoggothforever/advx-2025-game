using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LevelModel : MonoBehaviour
{
    public LevelModel Instance;
    // Start is called before the first frame update
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
