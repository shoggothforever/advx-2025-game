using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerButton : MonoBehaviour
{
    [Header("Õ‚π€")]
    public GameObject spriteOn;
    public GameObject spriteOff;
    public float delay = 1f;
    public bool ok = false;
    public void Awake()
    {
        spriteOn.SetActive(false);
        spriteOff.SetActive(true);
    }
    private void OnTriggerEnter2D(Collider2D c)
    {
        
        StartCoroutine(DelaySwitch());
    }
    public bool isOk()
    {
        return ok;
    }
    private IEnumerator DelaySwitch()
    {

        yield return new WaitForSeconds(delay);
        spriteOff.SetActive(false);
        spriteOn.SetActive(true);
        ok = true;

    }
}
