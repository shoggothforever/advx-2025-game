using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Core;
using SaveYourself.Mechanics;

public class UnRegisterArea : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D c)
    {
        bool needUnRegister = false;
        Debug.Log("trigger UnRegisterArea");
        if (c.name.StartsWith("reversible"))
        {
            if (c.CompareTag("Box"))
            {
                Debug.Log("unregister this reversible box");
                needUnRegister = true;
            }
        }
        if (needUnRegister)
        {
            TimeReverse.ITimeTrackable trackable = c.GetComponentInParent<TimeReverse.ITimeTrackable>();
            if (trackable != null) Debug.LogFormat("unregister this reversible object with id {0}", trackable.Id);
            TimeManager.Instance.UnRegiste(trackable);
        }
    }
}