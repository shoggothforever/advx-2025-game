using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Core;
using SaveYourself.Mechanics;

public class UnRegisterArea : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D c)
    {
        Debug.Log("trigger UnRegisterArea");
        if (c.name.StartsWith("reversible"))
        {

            Transform parent = c.transform.parent;          // ¸¸¼¶ Transform
            if (c.CompareTag("GhostPlayer"))
            {
                TimeManager.Instance.UnRegisterByID(1);
                Debug.Log("unrigister player reverse");

            }
            //else if (c.CompareTag("Box"))
            //{
            //    BaseBox box = GetFromCollider(c);
            //    if (box != null) Debug.Log("unregister this reversible box");
            //    TimeManager.Instance.UnRegister(box);
            //}
            //TimeReverse.ITimeTrackable trackable = c.GetComponentInParent<TimeReverse.ITimeTrackable>();
            //if (trackable != null) Debug.Log("unregister this reversible object");
            //TimeManager.Instance.UnRegister(trackable);
        }
    }
}