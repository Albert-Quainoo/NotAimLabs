using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public Text valuelabel; 

    
    public void OnSliderChanged (float value)
    {
        valuelabel.text = value.ToString();
    }

}
