using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    public Text ScoreText;
    public Score score;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
       if (ScoreText != null && score!= null)
       {
        ScoreText.text = string.Format("{0:N0}", Mathf.RoundToInt(score.score));
       }
    }
}
