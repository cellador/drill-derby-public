using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Public countdown that executes a callback once finished.
/// </summary>
public class Countdown
{
    public float val {get; private set;}

    public IEnumerator Start(float countdownValue, Action action)
    {
        val = countdownValue;
        while (val > 0f)
        {
            val = Mathf.Max(0f,val - Time.deltaTime);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        action();
    }    
}
