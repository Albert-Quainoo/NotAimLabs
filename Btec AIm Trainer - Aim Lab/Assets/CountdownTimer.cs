using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CountdownTimer : MonoBehaviour
{
    float currentTime = 0f;
    public float startingtime = 60f;
    public GameObject trObject;
    public GameObject AccuracyObject;
    public GameObject ScoreObject;
    public GameObject Crosshair;
    public GameObject finalScore;
    public GameObject Clicktorestart;


    [SerializeField] Text CountdownText;


    void Start()
    {
        currentTime = startingtime;
    }



    void Update()
    {
        currentTime -= 1 * Time.deltaTime;
        if (!(currentTime <= 0))
        {
            CountdownText.text = currentTime.ToString();
        }

        if (currentTime <= 0)
        {
            EndGame();
        }
    }


    void EndGame()
    {
        trObject.SetActive(false);
        Crosshair.SetActive(false);
        finalScore.SetActive(true);
        ScoreObject.GetComponent<RectTransform>().localPosition = Vector2.zero;
        CountdownText.text = "0.00";
        Clicktorestart.SetActive(true);


        if (Input.GetMouseButton(0))
        {
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        }

        //Scoretext


    }
}
