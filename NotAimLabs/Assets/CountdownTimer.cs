using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CountdownTimer : MonoBehaviour
{
    public static CountdownTimer Instance;
    private float currentTime = 0f;
    public float startingtime = 60f;
    
    // UI Elements
    public GameObject trObject;
    public GameObject AccuracyObject;
    public GameObject ScoreObject;
    public GameObject Crosshair;
    public GameObject finalScore;
    public GameObject Clicktorestart;
    public Text CountdownText;

    // Timer Visuals
    [SerializeField] private Color normalTimerColor = Color.white;
    [SerializeField] private Color warningTimerColor = Color.red;
    [SerializeField] private float warningThreshold = 10f;
    private bool isWarningActive = false;

    // Game State
    [SerializeField] private Score score;
    public bool isGameOver { get; private set; }
    private bool canRestart = false;
    public Target target;

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
        ResetTimer();
        isGameOver = false;
        
        // Hide end-game UI at start
        if (finalScore != null) finalScore.SetActive(false);
        if (Clicktorestart != null) Clicktorestart.SetActive(false);
        
        // Show in-game UI
        if (ScoreObject != null) ScoreObject.SetActive(true);
        if (AccuracyObject != null) AccuracyObject.SetActive(true);
        if (Crosshair != null) Crosshair.SetActive(true);
        if (trObject != null) trObject.SetActive(true);
    }

    void Update()
    {
        if (!isGameOver)
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                
                // Clamp the time to prevent negative values
                currentTime = Mathf.Max(0f, currentTime);

                // Update timer display
                UpdateTimerDisplay();

                // Check for game over
                if (currentTime <= 0)
                {
                    EndGame();
                }
            }
        }
        
        // Handle restart
        if (isGameOver && canRestart && Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void UpdateTimerDisplay()
    {
        if (CountdownText != null)
        {
            // Show minutes:seconds format if time is >= 60 seconds
            if (startingtime >= 60f)
            {
                int minutes = Mathf.FloorToInt(currentTime / 60f);
                int seconds = Mathf.FloorToInt(currentTime % 60f);
                CountdownText.text = string.Format("{0:}:{1:00}", minutes, seconds);
            }
            else
            {
                // Show seconds with decimal for times under 60 seconds
                CountdownText.text = currentTime.ToString("F1");
            }

            // Handle warning color change
            if (currentTime <= warningThreshold && !isWarningActive)
            {
                isWarningActive = true;
                CountdownText.color = warningTimerColor;
            }
        }
    }

    void EndGame()
    {
        if (isGameOver) return;
        isGameOver = true;

        // Hide gameplay elements
        if (trObject != null) trObject.SetActive(false);
        if (Crosshair != null) Crosshair.SetActive(false);
        if (ScoreObject != null) ScoreObject.SetActive(false);
        if (AccuracyObject != null) AccuracyObject.SetActive(false);

        // Show end-game UI
        if (finalScore != null)
        {
            finalScore.SetActive(true);
            Text finalScoreText = finalScore.GetComponent<Text>();
            if (finalScoreText != null && score != null)
            {
                finalScoreText.text = $"Final Score: {Mathf.RoundToInt(score.score)}\n" +
                                    $"Accuracy: {Mathf.RoundToInt(score.scoreAccuracy)}%";
            }
        }

        if (Clicktorestart != null)
        {
            Clicktorestart.SetActive(true);
        }

        if (CountdownText != null)
        {
            CountdownText.text = "0:00";
        }

        if (target != null)
        {
            target.DisableTarget();
        }

        StartCoroutine(RestartCooldown());
    }

    void ResetTimer()
    {
        currentTime = startingtime;
        isWarningActive = false;
        if (CountdownText != null)
        {
            CountdownText.color = normalTimerColor;
        }
    }

    IEnumerator RestartCooldown()
    {
        yield return new WaitForSeconds(0.5f);
        canRestart = true;
    }
}