using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Core;
public class TriggerButton : TimeReverse.ReversibleObject, TimeReverse.ITimeTrackable
{
    [Header("Õ‚π€")]
    public GameObject spriteOn;
    public GameObject spriteOff;
    public float delay = 0.1f;
    public bool ok = false;
    public new void Awake()
    {
        base.Awake();
        switchButton(ok);
    }
    public override void Start()
    {
        //GameManager.Instance.addTrack(this);
    }
    private void OnTriggerEnter2D(Collider2D c)
    {
        Debug.Log("enter button collider");
        StartCoroutine(DelaySwitch());
    }
    void Update()
    {
        if (ok &&!spriteOn.active)
        {
            DelaySwitch();
        }
        else if(!ok &&spriteOn.active)
        {
            switchButton(ok);
        }
    }
    public bool isOk()
    {
        return ok;
    }
    private IEnumerator DelaySwitch()
    {
        TimeManager.Instance.addHistory(this,RecordSnapshot());
        yield return new WaitForSeconds(delay);
        ok = true;
        switchButton(ok);


    }
    private void switchButton(bool ok)
    {
        spriteOn.SetActive(ok);
        spriteOff.SetActive(!ok);
    }
    public TimeReverse.TimedAction RecordSnapshot()
    {
        return new TimeReverse.TimedAction
        {
            time = TimeManager.Instance.currentTime,
            objId = Id,
            activated = ok,
        };
    }

    void TimeReverse.ITimeTrackable.ApplySnapshot(TimeReverse.TimedAction a)
    {
        ok=a.activated;
    }

    TimeReverse.ActionType TimeReverse.ITimeTrackable.GetActionType()
    {
        return TimeReverse.ActionType.Manual;
    }
}
