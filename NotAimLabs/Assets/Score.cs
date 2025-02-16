using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public static Score Instance;
    public float score {get; private set;}
    public float scoreAccuracy {get; set;}

    private int totalShots;
    private int successfulHits;
    
    // UI References
    public Text scoreText;
    public Text accuracyText;
    


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ResetScore();
    }

    public void AddHit()
    {
        successfulHits++;
        totalShots++;
        UpdateAccuracy();
    }

    public void AddMiss()
    {
        totalShots++;
        UpdateAccuracy();
    }

    private void UpdateAccuracy()
    {
        scoreAccuracy = totalShots > 0 ? ((float)successfulHits / totalShots) * 100f : 0f;
    }


   public void AddScore(float baseScore, float timeBonus, float accuracyBonus)
    {
       float totalBonus = CalculateTotalBonus(timeBonus, accuracyBonus);
       score += baseScore + totalBonus;
    }

    private float CalculateTotalBonus(float timeBonus, float accuracyBonus)
    {
        float normalizedTimeBonus = Mathf.Min(timeBonus,50f);

        float normalizedAccuracyBonus = (accuracyBonus / 100f) * 25f;

        return normalizedTimeBonus + normalizedAccuracyBonus;
    }

     public void DeductScore (int penalty)
     {
        score = Mathf.Max(0, score - penalty);
     }


    public void ResetScore()
    {
        score = 0f;
        scoreAccuracy = 0f;
        totalShots = 0;
        successfulHits = 0;
    }
}