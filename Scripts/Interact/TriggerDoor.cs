using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDoor : MonoBehaviour
{
    // Start is called before the first frame update
    TriggerButton[] btns;
    public bool isOpen = false;
    private Transform OriginPostion;
    void Start()
    {
        btns = GetComponent<TriggerButton[]>();
        OriginPostion = transform;
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
        Destroy(gameObject);
    }
    public virtual void Close()
    {
        if (!isOpen) return;
        isOpen = false;
        Instantiate(gameObject, OriginPostion);
    }
}
