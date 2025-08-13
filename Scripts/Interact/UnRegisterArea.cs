using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Core;
public class UnRegisterArea : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D c)
    {
        Debug.Log("trigger UnRegisterArea");
        if (c.name.StartsWith("reversible"))
        {
            Transform parent = c.transform.parent;          // ¸¸¼¶ Transform
            TimeReverse.ITimeTrackable trackable = parent?.GetComponent<TimeReverse.ITimeTrackable>();
            TimeManager.Instance.UnRegister(trackable);
        }
    }
}