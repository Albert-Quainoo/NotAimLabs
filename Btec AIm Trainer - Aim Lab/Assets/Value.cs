using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Value : MonoBehaviour
{

    public float value;
    Text PercentageText;

    [SerializeField] Text percentageText;


    void Start()
    {
      PercentageText = GetComponent <Text> ();
    }


    public void textUpdate(float value)
    {
        PercentageText.text = Mathf.RoundToInt(value * 100) + "%";
    }
}
