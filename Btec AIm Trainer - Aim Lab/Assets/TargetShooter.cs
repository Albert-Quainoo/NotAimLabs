using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetShooter : MonoBehaviour
{
    [SerializeField] Camera cam;
    public Score score;
    public float ScoreIncrement;
    public int hits;
    public int totalclick;
    public float Accuracy;
    public Text AccuracyText;
    public float timeSinceLastHit;
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            //
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Target Target = hit.collider.gameObject.GetComponent<Target>();


                if (Target != null)
                {
                    ScoreIncrement = 1 / timeSinceLastHit;
                    Target.Hit();

                    score.score += (float)Math.Round(ScoreIncrement * 10000, 1, MidpointRounding.AwayFromZero);
                    hits++;
                    timeSinceLastHit = 0;
                }

                totalclick++;
            }

        }
        if (totalclick != 0 && hits != 0)
        {
            Accuracy = (float)hits / (float)totalclick * 100f;
            AccuracyText.text = Math.Round(Accuracy, 1, MidpointRounding.AwayFromZero).ToString();
        }

        timeSinceLastHit += Time.deltaTime + 1;
    }



}
