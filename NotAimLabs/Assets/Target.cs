using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


public class Target : MonoBehaviour
{
    private static int totalShots = 0;
    private static int successfulHits = 0;
    private static Value accuracyDisplay;

    public void Hit()
    {

        transform.position = TargetBounds.Instance.GetRandomPosition();

        // Play sound effect on hit
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.Play();
    }

    public static void RecordShot()
    {
        totalShots++;
        UpdateAccuracy();
    }

    private static void UpdateAccuracy()
    {
        if (accuracyDisplay != null)
        {
            if (totalShots == 0)
            {
                accuracyDisplay.UpdateAccuracy(0f);
            }
            else
            {
                float accuracy = (float)successfulHits / totalShots;
                accuracyDisplay.UpdateAccuracy(accuracy);
            }
        }
    }

    public static void ResetAccuracy()
    {
        totalShots = 0;
        successfulHits = 0;
        if (accuracyDisplay != null)
        {
            accuracyDisplay.UpdateAccuracy(0f);
        }
    }

    // Disable the target
    public void DisableTarget()
    {
        gameObject.SetActive(false);
    }
}