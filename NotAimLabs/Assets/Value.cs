using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Value : MonoBehaviour
{

    
     private Score ScoreScript;
     private Text accuracyText;

    [Obsolete]
    void Start()
  {
      accuracyText = GetComponent<Text>();
      if (accuracyText == null)
      {
        Debug.LogError("No Text Component found on " + gameObject.name);
        return;
      }
      ScoreScript = FindObjectOfType<Score>();
      if (ScoreScript == null)
      {
        Debug.LogError("No Score script found in scene");
        return;
      }
       
     
  }

    void Update()
    {
       if (accuracyText != null && ScoreScript != null)
       {
        accuracyText.text = Mathf.RoundToInt(ScoreScript.scoreAccuracy).ToString() + "%";
       }
    }
    public void UpdateAccuracy(float value)
{
    if (ScoreScript != null)
    {
        ScoreScript.scoreAccuracy = Mathf.Clamp01(value)* 100f;
    }
  }
     
   public float GetCurrentAccuracy()
   {
    if (ScoreScript != null)
    {
        return ScoreScript.scoreAccuracy / 100f;
    }
    return 0f;
   
    }
}

