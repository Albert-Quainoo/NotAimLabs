using System;
using UnityEngine;

public class Percentage : MonoBehaviour

{
    public Value percentageDisplay;
    
    void Update()
    {
        float value = (Mathf.Sin(Time.time)+ 1f) / 2f;
        percentageDisplay.UpdateAccuracy(value);
    }

}

