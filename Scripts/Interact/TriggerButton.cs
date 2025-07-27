using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerButton : MonoBehaviour
{
    public bool ok = false;
    private void OnCollisionEnter2D(Collision2D c)
    {
        ok = true;
    }
    public bool isOk()
    {
        return ok;
    }
}
