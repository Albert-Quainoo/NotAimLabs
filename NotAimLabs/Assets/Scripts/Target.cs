using UnityEngine;

public class Target : MonoBehaviour
{
    private static int totalShots = 0;
    private static int successfulHits = 0;
    private static Value accuracyDisplay;

    private void Awake()
    {
        if (accuracyDisplay == null)
        {
            accuracyDisplay = FindAnyObjectByType<Value>();
            if (accuracyDisplay == null)
            {
                Debug.LogWarning("Target cannot find Accuracy Display (Value script) in the scene.");
            }
        }
    }

    public void Hit()
    {
        if (TargetBounds.Instance != null)
        {
            transform.position = TargetBounds.Instance.GetRandomPosition();
        }
        else
        {
            Debug.LogError("TargetBounds.Instance is null! Cannot reposition target.");
        }

        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.loop = false;
            audioSource.Play();
        }
    }

    public static void RecordShot()
    {
        totalShots++;
        UpdateAccuracy();
    }

    public static void RecordHit()
    {
        successfulHits++;
    }

    private static void UpdateAccuracy()
    {
        if (accuracyDisplay != null)
        {
            float accuracy = 0f;
            if (totalShots > 0)
            {
                accuracy = (float)successfulHits / totalShots;
            }
            accuracyDisplay.UpdateAccuracy(accuracy);
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
        Debug.Log("Accuracy Reset.");
    }

    public void DisableTarget()
    {
        gameObject.SetActive(false);
    }
}
