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
        if (c.CompareTag("Box"))
        {
            needUnRegister = true;
        }
        if (needUnRegister)
        {
            TimeReverse.ITimeTrackable trackable = c.GetComponentInParent<TimeReverse.ITimeTrackable>();
            TimeManager.Instance.UnRegiste(trackable);
        }
    }
}