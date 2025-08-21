using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Core;
public class BaseReversibleBox : TimeReverse.ReversibleObject, TimeReverse.ITimeTrackable
{
    Rigidbody2D rb;
    public override void Start()
    {
        GameManager.Instance.addTrack(this);
        rb = GetComponent<Rigidbody2D>();
    }
    public TimeReverse.TimedAction RecordSnapshot()
    {
        return new TimeReverse.TimedAction
        {
            time = TimeManager.Instance.currentTime,
            objId = Id,
            pos = transform.position,
            velocity = rb.velocity,
            rotation = rb.rotation
            //payload = JsonUtility.ToJson(transform.position)
        };
    }
    public void ApplySnapshot(TimeReverse.TimedAction a)
    {
        //rb.MovePosition(a.pos);
        transform.position = Vector3.Lerp(transform.position, a.pos, 0.4f);
    }
    TimeReverse.ActionType TimeReverse.ITimeTrackable.GetActionType()
    {
        return TimeReverse.ActionType.Position;     // Ã¶¾Ù£ºPosition, AnimatorBool, AnimatorTrigger...
    }
    string TimeReverse.ITimeTrackable.Name()
    {
        return "base box";
    }
}