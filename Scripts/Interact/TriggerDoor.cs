using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Core;
public class TriggerDoor : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject door;
    public TriggerButton[] btns;
    public bool isOpen = false;
    void Start()
    {
        //btns = GetComponent<TriggerButton[]>();
        door.SetActive(!isOpen);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var btn in btns)
        {
            if (!btn.isOk())
            {
                Close();
                return;
            }
        }
        Open();

    }
    public virtual void Open()
    {
        if (isOpen) 
            return;
        isOpen = true;
        door.SetActive(false);
    }
    public virtual void Close()
    {
        if (!isOpen) return;
        isOpen = false;
        door.SetActive(true);
    }

}
